using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Services.Controllers.DataSync
{
    /// <summary>
    /// Service implementation of <see cref="I"/>
    /// </summary>
    [Authorize(Policy = "HasMandatoryRole")]
    public abstract class LegacySyncControllerBase<TModel> : LegacySyncControllerBase<TModel, TModel>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logic">The logic layer</param>
        protected LegacySyncControllerBase(ICrudable<TModel, string> logic)
            : base(logic)
        {
        }
    }

    /// <summary>
    /// Service implementation of <see cref="I"/>
    /// </summary>
    [Authorize(Policy = "HasMandatoryRole")]
    public abstract class LegacySyncControllerBase<TModelCreate, TModel> : ControllerBase where TModel : TModelCreate
    {
        /// <summary>
        /// The capability for this controller
        /// </summary>
        protected readonly ICrudable<TModel, string> Logic;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logic">The logic layer</param>
        protected LegacySyncControllerBase(ICrudable<TModel, string> logic)
        {
            InternalContract.RequireNotNull(logic, nameof(logic));
            Logic = logic;
        }

        /// <summary>
        /// GET for a synchronized object, according to the contract in http://lever.xlent-fulcrum.info/wiki/XLENT_Match
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<TModel>> LegacyReadAsync(string id,
            CancellationToken token = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            try
            {
                if (!(Logic is IRead<TModel, string> readLogic))
                {
                    return StatusCode(500, $"{Logic.GetType().FullName} must implement IRead");
                }

                var objectData = await readLogic.ReadAsync(id, token);
                if (objectData == null)
                {
                    return NotFound("\"CBF83F95-8A30-4887-B57B-3E471246D825\"");
                }

                return Ok(JObject.FromObject(objectData, JsonSerializer.Create(JsonSerializerSettings())));
            }
            catch (FulcrumNotFoundException)
            {
                return NotFound("\"CBF83F95-8A30-4887-B57B-3E471246D825\"");
            }
            catch (FulcrumForbiddenAccessException e)
            {
                return StatusCode(403, e);
            }
            catch (FulcrumException)
            {
                throw;
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        /// <summary>
        /// GET for a synchronized object, according to the contract in http://lever.xlent-fulcrum.info/wiki/XLENT_Match
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult> LegacyUpdateAsync(string id, [FromBody] TModel item,
            CancellationToken token = default)
        {
            try
            {
                if (!(Logic is IUpdate<TModel, string> updateLogic))
                {
                    return StatusCode(500, $"{Logic.GetType().FullName} must implement IUpdate");
                }

                await updateLogic.UpdateAsync(id, item, token);
                return Ok();
            }
            catch (FulcrumNotFoundException)
            {
                return NotFound("CBF83F95-8A30-4887-B57B-3E471246D825");
            }
            catch (FulcrumForbiddenAccessException e)
            {
                return StatusCode(403, e);
            }
            catch (FulcrumException)
            {
                throw;
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        /// <summary>
        /// POST for a synchronized object, according to the contract in http://lever.xlent-fulcrum.info/wiki/XLENT_Match
        /// </summary>
        [HttpPost("")]
        public async Task<ActionResult<string>> LegacyCreateAsync([FromBody] TModelCreate item,
            CancellationToken token = default)
        {
            try
            {
                if (!(Logic is ICreate<TModelCreate, TModel, string> createLogic))
                {
                    return StatusCode(500, $"{Logic.GetType().FullName} must implement ICreate");
                }

                var id = await createLogic.CreateAsync(item, token);
                FulcrumAssert.IsNotNullOrWhiteSpace(id, CodeLocation.AsString());
                return Ok(id);
            }
            catch (FulcrumNotFoundException)
            {
                return NotFound("CBF83F95-8A30-4887-B57B-3E471246D825");
            }
            catch (FulcrumForbiddenAccessException e)
            {
                return StatusCode(403, e);
            }
            catch (FulcrumException)
            {
                throw;
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        private static JsonSerializerSettings JsonSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                DateParseHandling = DateParseHandling.None,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                ContractResolver = new Microsoft.Rest.Serialization.ReadOnlyJsonContractResolver(),
                Converters = new System.Collections.Generic.List<JsonConverter>
                {
                    new Microsoft.Rest.Serialization.Iso8601TimeSpanConverter()
                }
            };
        }
    }
}
