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
    /// Concept
    /// </summary>
    [DataContract]
    public class Concept :  IEquatable<Concept>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Concept" /> class.
        /// </summary>
        /// <param name="Id">Id.</param>
        /// <param name="TechnicalName">TechnicalName.</param>
        /// <param name="FriendlyName">FriendlyName.</param>
        /// <param name="DefaultContextId">DefaultContextId.</param>
        /// <param name="CanCreateInstancesManually">CanCreateInstancesManually.</param>
        /// <param name="CanCreateInstancesAutomatically">CanCreateInstancesAutomatically.</param>
        public Concept(Guid? Id = default, string TechnicalName = default, string FriendlyName = default, Guid? DefaultContextId = default, bool? CanCreateInstancesManually = default, bool? CanCreateInstancesAutomatically = default)
        {
            this.Id = Id;
            this.TechnicalName = TechnicalName;
            this.FriendlyName = FriendlyName;
            this.DefaultContextId = DefaultContextId;
            this.CanCreateInstancesManually = CanCreateInstancesManually;
            this.CanCreateInstancesAutomatically = CanCreateInstancesAutomatically;
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
        /// Gets or Sets DefaultContextId
        /// </summary>
        [DataMember(Name="DefaultContextId", EmitDefaultValue=false)]
        public Guid? DefaultContextId { get; set; }
        /// <summary>
        /// Gets or Sets CanCreateInstancesManually
        /// </summary>
        [DataMember(Name="CanCreateInstancesManually", EmitDefaultValue=false)]
        public bool? CanCreateInstancesManually { get; set; }
        /// <summary>
        /// Gets or Sets CanCreateInstancesAutomatically
        /// </summary>
        [DataMember(Name="CanCreateInstancesAutomatically", EmitDefaultValue=false)]
        public bool? CanCreateInstancesAutomatically { get; set; }
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
            sb.Append("class Concept {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  TechnicalName: ").Append(TechnicalName).Append("\n");
            sb.Append("  FriendlyName: ").Append(FriendlyName).Append("\n");
            sb.Append("  DefaultContextId: ").Append(DefaultContextId).Append("\n");
            sb.Append("  CanCreateInstancesManually: ").Append(CanCreateInstancesManually).Append("\n");
            sb.Append("  CanCreateInstancesAutomatically: ").Append(CanCreateInstancesAutomatically).Append("\n");
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
            return Equals(obj as Concept);
        }

        /// <summary>
        /// Returns true if Concept instances are equal
        /// </summary>
        /// <param name="other">Instance of Concept to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(Concept other)
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
                    DefaultContextId == other.DefaultContextId ||
                    DefaultContextId != null &&
                    DefaultContextId.Equals(other.DefaultContextId)
                ) && 
                (
                    CanCreateInstancesManually == other.CanCreateInstancesManually ||
                    CanCreateInstancesManually != null &&
                    CanCreateInstancesManually.Equals(other.CanCreateInstancesManually)
                ) && 
                (
                    CanCreateInstancesAutomatically == other.CanCreateInstancesAutomatically ||
                    CanCreateInstancesAutomatically != null &&
                    CanCreateInstancesAutomatically.Equals(other.CanCreateInstancesAutomatically)
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
                if (DefaultContextId != null)
                    hash = hash * 59 + DefaultContextId.GetHashCode();
                if (CanCreateInstancesManually != null)
                    hash = hash * 59 + CanCreateInstancesManually.GetHashCode();
                if (CanCreateInstancesAutomatically != null)
                    hash = hash * 59 + CanCreateInstancesAutomatically.GetHashCode();
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
