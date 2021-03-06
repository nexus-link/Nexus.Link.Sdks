/* 
 * Xlent.Lever.KeyTranslator.Facade
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

namespace Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Models
{
    /// <summary>
    /// Association
    /// </summary>
    [DataContract]
    public class Association :  IEquatable<Association>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Association" /> class.
        /// </summary>
        /// <param name="SourcePath">SourcePath.</param>
        /// <param name="TargetContextName">TargetContextName.</param>
        /// <param name="TargetClientName">TargetClientName.</param>
        /// <param name="TargetValue">TargetValue.</param>
        /// <param name="LockId">LockId.</param>
        public Association(string SourcePath = default, string TargetContextName = default, string TargetClientName = default, string TargetValue = default, string LockId = default)
        {
            this.SourcePath = SourcePath;
            this.TargetContextName = TargetContextName;
            this.TargetClientName = TargetClientName;
            this.TargetValue = TargetValue;
            this.LockId = LockId;
        }
        
        /// <summary>
        /// Gets or Sets SourcePath
        /// </summary>
        [DataMember(Name="SourcePath", EmitDefaultValue=false)]
        public string SourcePath { get; set; }
        /// <summary>
        /// Gets or Sets TargetContextName
        /// </summary>
        [DataMember(Name="TargetContextName", EmitDefaultValue=false)]
        public string TargetContextName { get; set; }
        /// <summary>
        /// Gets or Sets TargetClientName
        /// </summary>
        [DataMember(Name="TargetClientName", EmitDefaultValue=false)]
        public string TargetClientName { get; set; }
        /// <summary>
        /// Gets or Sets TargetValue
        /// </summary>
        [DataMember(Name="TargetValue", EmitDefaultValue=false)]
        public string TargetValue { get; set; }
        /// <summary>
        /// Gets or Sets LockId
        /// </summary>
        [DataMember(Name="LockId", EmitDefaultValue=false)]
        public string LockId { get; set; }
        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class Association {\n");
            sb.Append("  SourcePath: ").Append(SourcePath).Append("\n");
            sb.Append("  TargetContextName: ").Append(TargetContextName).Append("\n");
            sb.Append("  TargetClientName: ").Append(TargetClientName).Append("\n");
            sb.Append("  TargetValue: ").Append(TargetValue).Append("\n");
            sb.Append("  LockId: ").Append(LockId).Append("\n");
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
            return Equals(obj as Association);
        }

        /// <summary>
        /// Returns true if Association instances are equal
        /// </summary>
        /// <param name="other">Instance of Association to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(Association other)
        {
            // credit: http://stackoverflow.com/a/10454552/677735
            if (other == null)
                return false;

            return 
                (
                    SourcePath == other.SourcePath ||
                    SourcePath != null &&
                    SourcePath.Equals(other.SourcePath)
                ) && 
                (
                    TargetContextName == other.TargetContextName ||
                    TargetContextName != null &&
                    TargetContextName.Equals(other.TargetContextName)
                ) && 
                (
                    TargetClientName == other.TargetClientName ||
                    TargetClientName != null &&
                    TargetClientName.Equals(other.TargetClientName)
                ) && 
                (
                    TargetValue == other.TargetValue ||
                    TargetValue != null &&
                    TargetValue.Equals(other.TargetValue)
                ) && 
                (
                    LockId == other.LockId ||
                    LockId != null &&
                    LockId.Equals(other.LockId)
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
                if (SourcePath != null)
                    hash = hash * 59 + SourcePath.GetHashCode();
                if (TargetContextName != null)
                    hash = hash * 59 + TargetContextName.GetHashCode();
                if (TargetClientName != null)
                    hash = hash * 59 + TargetClientName.GetHashCode();
                if (TargetValue != null)
                    hash = hash * 59 + TargetValue.GetHashCode();
                if (LockId != null)
                    hash = hash * 59 + LockId.GetHashCode();
                return hash;
            }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        { 
            yield break;
        }
    }

}
