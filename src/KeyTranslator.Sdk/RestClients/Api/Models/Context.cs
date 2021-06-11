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
    /// Context
    /// </summary>
    [DataContract]
    public partial class Context :  IEquatable<Context>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Context" /> class.
        /// </summary>
        /// <param name="Id">Id.</param>
        /// <param name="TechnicalName">TechnicalName.</param>
        /// <param name="FriendlyName">FriendlyName.</param>
        /// <param name="IsNewestPreferred">IsNewestPreferred.</param>
        /// <param name="CanCreateInstancesManually">CanCreateInstancesManually.</param>
        /// <param name="CanCreateInstancesAutomatically">CanCreateInstancesAutomatically.</param>
        /// <param name="HasAutomaticInstanceGeneration">HasAutomaticInstanceGeneration.</param>
        /// <param name="GeneratedNumberOfCharacters">GeneratedNumberOfCharacters.</param>
        /// <param name="GeneratedAllowedCharacters">GeneratedAllowedCharacters.</param>
        /// <param name="IsCaseSensitive">IsCaseSensitive.</param>
        public Context(Guid? Id = default, string TechnicalName = default, string FriendlyName = default, bool? IsNewestPreferred = default, bool? CanCreateInstancesManually = default, bool? CanCreateInstancesAutomatically = default, bool? HasAutomaticInstanceGeneration = default, int? GeneratedNumberOfCharacters = default, string GeneratedAllowedCharacters = default, bool? IsCaseSensitive = default)
        {
            this.Id = Id;
            this.TechnicalName = TechnicalName;
            this.FriendlyName = FriendlyName;
            this.IsNewestPreferred = IsNewestPreferred;
            this.CanCreateInstancesManually = CanCreateInstancesManually;
            this.CanCreateInstancesAutomatically = CanCreateInstancesAutomatically;
            this.HasAutomaticInstanceGeneration = HasAutomaticInstanceGeneration;
            this.GeneratedNumberOfCharacters = GeneratedNumberOfCharacters;
            this.GeneratedAllowedCharacters = GeneratedAllowedCharacters;
            this.IsCaseSensitive = IsCaseSensitive;
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
        /// Gets or Sets IsNewestPreferred
        /// </summary>
        [DataMember(Name="IsNewestPreferred", EmitDefaultValue=false)]
        public bool? IsNewestPreferred { get; set; }
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
        /// Gets or Sets HasAutomaticInstanceGeneration
        /// </summary>
        [DataMember(Name = "HasAutomaticInstanceGeneration", EmitDefaultValue = false)]
        public bool? HasAutomaticInstanceGeneration { get; set; }
        /// <summary>
        /// Gets or Sets GeneratedNumberOfCharacters
        /// </summary>
        [DataMember(Name = "GeneratedNumberOfCharacters", EmitDefaultValue = false)]
        public int? GeneratedNumberOfCharacters { get; set; }
        /// <summary>
        /// Gets or Sets GeneratedAllowedCharacters
        /// </summary>
        [DataMember(Name = "GeneratedAllowedCharacters", EmitDefaultValue = false)]
        public string GeneratedAllowedCharacters { get; set; }
        /// <summary>
        /// Gets or Sets IsCaseSensitive
        /// </summary>
        [DataMember(Name="IsCaseSensitive", EmitDefaultValue=false)]
        public bool? IsCaseSensitive { get; set; }
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
            sb.Append("class Context {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  TechnicalName: ").Append(TechnicalName).Append("\n");
            sb.Append("  FriendlyName: ").Append(FriendlyName).Append("\n");
            sb.Append("  IsNewestPreferred: ").Append(IsNewestPreferred).Append("\n");
            sb.Append("  CanCreateInstancesManually: ").Append(CanCreateInstancesManually).Append("\n");
            sb.Append("  CanCreateInstancesAutomatically: ").Append(CanCreateInstancesAutomatically).Append("\n");
            sb.Append("  HasAutomaticInstanceGeneration: ").Append(HasAutomaticInstanceGeneration).Append("\n");
            sb.Append("  GeneratedNumberOfCharacters: ").Append(GeneratedNumberOfCharacters).Append("\n");
            sb.Append("  GeneratedAllowedCharacters: ").Append(GeneratedAllowedCharacters).Append("\n");
            sb.Append("  IsCaseSensitive: ").Append(IsCaseSensitive).Append("\n");
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
            return this.Equals(obj as Context);
        }

        /// <summary>
        /// Returns true if Context instances are equal
        /// </summary>
        /// <param name="other">Instance of Context to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(Context other)
        {
            // credit: http://stackoverflow.com/a/10454552/677735
            if (other == null)
                return false;

            return 
                (
                    this.Id == other.Id ||
                    this.Id != null &&
                    this.Id.Equals(other.Id)
                ) && 
                (
                    this.TechnicalName == other.TechnicalName ||
                    this.TechnicalName != null &&
                    this.TechnicalName.Equals(other.TechnicalName)
                ) && 
                (
                    this.FriendlyName == other.FriendlyName ||
                    this.FriendlyName != null &&
                    this.FriendlyName.Equals(other.FriendlyName)
                ) && 
                (
                    this.IsNewestPreferred == other.IsNewestPreferred ||
                    this.IsNewestPreferred != null &&
                    this.IsNewestPreferred.Equals(other.IsNewestPreferred)
                ) && 
                (
                    this.CanCreateInstancesManually == other.CanCreateInstancesManually ||
                    this.CanCreateInstancesManually != null &&
                    this.CanCreateInstancesManually.Equals(other.CanCreateInstancesManually)
                ) && 
                (
                    this.CanCreateInstancesAutomatically == other.CanCreateInstancesAutomatically ||
                    this.CanCreateInstancesAutomatically != null &&
                    this.CanCreateInstancesAutomatically.Equals(other.CanCreateInstancesAutomatically)
                ) &&
                (
                    this.HasAutomaticInstanceGeneration == other.HasAutomaticInstanceGeneration ||
                    this.HasAutomaticInstanceGeneration != null &&
                    this.HasAutomaticInstanceGeneration.Equals(other.HasAutomaticInstanceGeneration)
                ) &&
                (
                    this.GeneratedNumberOfCharacters == other.GeneratedNumberOfCharacters ||
                    this.GeneratedNumberOfCharacters != null &&
                    this.GeneratedNumberOfCharacters.Equals(other.GeneratedNumberOfCharacters)
                ) &&
                (
                    this.GeneratedAllowedCharacters == other.GeneratedAllowedCharacters ||
                    this.GeneratedAllowedCharacters != null &&
                    this.GeneratedAllowedCharacters.Equals(other.GeneratedAllowedCharacters)
                ) &&
                (
                    this.IsCaseSensitive == other.IsCaseSensitive ||
                    this.IsCaseSensitive != null &&
                    this.IsCaseSensitive.Equals(other.IsCaseSensitive)
                ) && 
                (
                    this.VersionTag == other.VersionTag ||
                    this.VersionTag != null &&
                    this.VersionTag.Equals(other.VersionTag)
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
                int hash = 41;
                // Suitable nullity checks etc, of course :)
                if (this.Id != null)
                    hash = hash * 59 + this.Id.GetHashCode();
                if (this.TechnicalName != null)
                    hash = hash * 59 + this.TechnicalName.GetHashCode();
                if (this.FriendlyName != null)
                    hash = hash * 59 + this.FriendlyName.GetHashCode();
                if (this.IsNewestPreferred != null)
                    hash = hash * 59 + this.IsNewestPreferred.GetHashCode();
                if (this.CanCreateInstancesManually != null)
                    hash = hash * 59 + this.CanCreateInstancesManually.GetHashCode();
                if (this.CanCreateInstancesAutomatically != null)
                    hash = hash * 59 + this.CanCreateInstancesAutomatically.GetHashCode();
                if (this.HasAutomaticInstanceGeneration != null)
                    hash = hash * 59 + this.HasAutomaticInstanceGeneration.GetHashCode();
                if (this.GeneratedNumberOfCharacters != null)
                    hash = hash * 59 + this.GeneratedNumberOfCharacters.GetHashCode();
                if (this.GeneratedAllowedCharacters != null)
                    hash = hash * 59 + this.GeneratedAllowedCharacters.GetHashCode();
                if (this.IsCaseSensitive != null)
                    hash = hash * 59 + this.IsCaseSensitive.GetHashCode();
                if (this.VersionTag != null)
                    hash = hash * 59 + this.VersionTag.GetHashCode();
                return hash;
            }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        { 
            yield break;
        }
    }

}
