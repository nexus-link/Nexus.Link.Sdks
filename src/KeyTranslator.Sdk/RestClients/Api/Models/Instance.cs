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
    /// Instance
    /// </summary>
    [DataContract]
    public class Instance :  IEquatable<Instance>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Instance" /> class.
        /// </summary>
        /// <param name="Id">Id.</param>
        /// <param name="ConceptId">ConceptId.</param>
        /// <param name="ContextId">ContextId.</param>
        /// <param name="FormId">FormId.</param>
        /// <param name="Value">Value.</param>
        /// <param name="IsPreferred">IsPreferred.</param>
        /// <param name="IsValid">IsValid.</param>
        public Instance(Guid? Id = default, Guid? ConceptId = default, Guid? ContextId = default, Guid? FormId = default, string Value = default, bool? IsPreferred = default, bool? IsValid = default)
        {
            this.Id = Id;
            this.ConceptId = ConceptId;
            this.ContextId = ContextId;
            this.FormId = FormId;
            this.Value = Value;
            this.IsPreferred = IsPreferred;
            this.IsValid = IsValid;
        }
        
        /// <summary>
        /// Gets or Sets Id
        /// </summary>
        [DataMember(Name="Id", EmitDefaultValue=false)]
        public Guid? Id { get; set; }
        /// <summary>
        /// Gets or Sets ConceptId
        /// </summary>
        [DataMember(Name="ConceptId", EmitDefaultValue=false)]
        public Guid? ConceptId { get; set; }
        /// <summary>
        /// Gets or Sets ContextId
        /// </summary>
        [DataMember(Name="ContextId", EmitDefaultValue=false)]
        public Guid? ContextId { get; set; }
        /// <summary>
        /// Gets or Sets FormId
        /// </summary>
        [DataMember(Name="FormId", EmitDefaultValue=false)]
        public Guid? FormId { get; set; }
        /// <summary>
        /// Gets or Sets Value
        /// </summary>
        [DataMember(Name="Value", EmitDefaultValue=false)]
        public string Value { get; set; }
        /// <summary>
        /// Gets or Sets IsPreferred
        /// </summary>
        [DataMember(Name="IsPreferred", EmitDefaultValue=false)]
        public bool? IsPreferred { get; set; }
        /// <summary>
        /// Gets or Sets IsValid
        /// </summary>
        [DataMember(Name="IsValid", EmitDefaultValue=false)]
        public bool? IsValid { get; set; }
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
            sb.Append("class Instance {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  ConceptId: ").Append(ConceptId).Append("\n");
            sb.Append("  ContextId: ").Append(ContextId).Append("\n");
            sb.Append("  FormId: ").Append(FormId).Append("\n");
            sb.Append("  Value: ").Append(Value).Append("\n");
            sb.Append("  IsPreferred: ").Append(IsPreferred).Append("\n");
            sb.Append("  IsValid: ").Append(IsValid).Append("\n");
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
            return Equals(obj as Instance);
        }

        /// <summary>
        /// Returns true if Instance instances are equal
        /// </summary>
        /// <param name="other">Instance of Instance to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(Instance other)
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
                    ConceptId == other.ConceptId ||
                    ConceptId != null &&
                    ConceptId.Equals(other.ConceptId)
                ) && 
                (
                    ContextId == other.ContextId ||
                    ContextId != null &&
                    ContextId.Equals(other.ContextId)
                ) && 
                (
                    FormId == other.FormId ||
                    FormId != null &&
                    FormId.Equals(other.FormId)
                ) && 
                (
                    Value == other.Value ||
                    Value != null &&
                    Value.Equals(other.Value)
                ) && 
                (
                    IsPreferred == other.IsPreferred ||
                    IsPreferred != null &&
                    IsPreferred.Equals(other.IsPreferred)
                ) && 
                (
                    IsValid == other.IsValid ||
                    IsValid != null &&
                    IsValid.Equals(other.IsValid)
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
                if (ConceptId != null)
                    hash = hash * 59 + ConceptId.GetHashCode();
                if (ContextId != null)
                    hash = hash * 59 + ContextId.GetHashCode();
                if (FormId != null)
                    hash = hash * 59 + FormId.GetHashCode();
                if (Value != null)
                    hash = hash * 59 + Value.GetHashCode();
                if (IsPreferred != null)
                    hash = hash * 59 + IsPreferred.GetHashCode();
                if (IsValid != null)
                    hash = hash * 59 + IsValid.GetHashCode();
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
