/* 
 * Xlent.Lever.KeyTranslator.Api.WebApi
 *
 * No description provided (generated by Swagger Codegen https://github.com/swagger-api/swagger-codegen)
 *
 * OpenAPI spec version: v1
 * 
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace Nexus.Link.KeyTranslator.Sdk.RestClients.Api.Models
{
    /// <summary>
    /// ModelClient
    /// </summary>
    [DataContract]
    public class Client :  IEquatable<Client>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Client" /> class.
        /// </summary>
        /// <param name="Id">Id.</param>
        /// <param name="TechnicalName">TechnicalName.</param>
        /// <param name="FriendlyName">FriendlyName.</param>
        public Client(Guid? Id = default, string TechnicalName = default, string FriendlyName = default)
        {
            this.Id = Id;
            this.TechnicalName = TechnicalName;
            this.FriendlyName = FriendlyName;
        }
        
        /// <summary>
        /// Gets or Sets Id
        /// </summary>
        [DataMember(Name="Id", EmitDefaultValue=false)]
        public Guid? Id { get; set; }
        /// <summary>
        /// Gets or Sets TechnicalName
        /// </summary>
        [DataMember(Name="TechnicalName", EmitDefaultValue=false)]
        public string TechnicalName { get; set; }
        /// <summary>
        /// Gets or Sets FriendlyName
        /// </summary>
        [DataMember(Name="FriendlyName", EmitDefaultValue=false)]
        public string FriendlyName { get; set; }
        /// <summary>
        /// Gets or Sets VersionTag
        /// </summary>
        [DataMember(Name="VersionTag", EmitDefaultValue=false)]
        public Guid? VersionTag { get; private set; }
        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class ModelClient {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  TechnicalName: ").Append(TechnicalName).Append("\n");
            sb.Append("  FriendlyName: ").Append(FriendlyName).Append("\n");
            sb.Append("  VersionTag: ").Append(VersionTag).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
  
        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="obj">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object obj)
        {
            // credit: http://stackoverflow.com/a/10454552/677735
            return Equals(obj as Client);
        }

        /// <summary>
        /// Returns true if ModelClient instances are equal
        /// </summary>
        /// <param name="other">Instance of ModelClient to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(Client other)
        {
            // credit: http://stackoverflow.com/a/10454552/677735
            if (other == null)
                return false;

            return 
                (
                    Id == other.Id ||
                    Id != null &&
                    Id.Equals(other.Id)
                ) && 
                (
                    TechnicalName == other.TechnicalName ||
                    TechnicalName != null &&
                    TechnicalName.Equals(other.TechnicalName)
                ) && 
                (
                    FriendlyName == other.FriendlyName ||
                    FriendlyName != null &&
                    FriendlyName.Equals(other.FriendlyName)
                ) && 
                (
                    VersionTag == other.VersionTag ||
                    VersionTag != null &&
                    VersionTag.Equals(other.VersionTag)
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            // credit: http://stackoverflow.com/a/263416/677735
            unchecked // Overflow is fine, just wrap
            {
                var hash = 41;
                // Suitable nullity checks etc, of course :)
                if (Id != null)
                    hash = hash * 59 + Id.GetHashCode();
                if (TechnicalName != null)
                    hash = hash * 59 + TechnicalName.GetHashCode();
                if (FriendlyName != null)
                    hash = hash * 59 + FriendlyName.GetHashCode();
                if (VersionTag != null)
                    hash = hash * 59 + VersionTag.GetHashCode();
                return hash;
            }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        { 
            yield break;
        }
    }

}
