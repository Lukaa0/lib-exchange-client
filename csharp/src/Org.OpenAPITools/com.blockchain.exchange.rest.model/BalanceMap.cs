/* 
 * Blockchain.com Exchange REST API
 *
 * ## Introduction Welcome to Blockchain.com's Exchange API and developer documentation. \\ These documents detail and give examples of various functionality offered by the API such as receiving real time market data, requesting balance information and performing trades. ## To Get Started Create or log into your existing Blockchain.com Exchange account \\ Select API from the drop down menu \\ Fill out form and click “Create New API Key Now” \\ Once generated you can view your keys under API Settings. \\ Please be aware that the API key can only be used once it was verified via email.  The API key must be set via the \\ `X-API-Token`\\ header.  The base URL to be used for all calls is \\ `https://api.blockchain.com/v3/exchange`  Autogenerated clients for this API can be found [here](https://github.com/blockchain/lib-exchange-client). 
 *
 * The version of the OpenAPI document: 1.0.0
 * 
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */

using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using OpenAPIDateConverter = Org.OpenAPITools.Client.OpenAPIDateConverter;

namespace Org.OpenAPITools.com.blockchain.exchange.rest.model
{
    /// <summary>
    /// BalanceMap
    /// </summary>
    [DataContract]
    public partial class BalanceMap : Dictionary<String, List>,  IEquatable<BalanceMap>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BalanceMap" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected BalanceMap() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="BalanceMap" /> class.
        /// </summary>
        /// <param name="primary">primary (required).</param>
        public BalanceMap(List<Balance> primary = default(List<Balance>)) : base()
        {
            // to ensure "primary" is required (not null)
            if (primary == null)
            {
                throw new InvalidDataException("primary is a required property for BalanceMap and cannot be null");
            }
            else
            {
                this.Primary = primary;
            }
            
        }
        
        /// <summary>
        /// Gets or Sets Primary
        /// </summary>
        [DataMember(Name="primary", EmitDefaultValue=true)]
        public List<Balance> Primary { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class BalanceMap {\n");
            sb.Append("  ").Append(base.ToString().Replace("\n", "\n  ")).Append("\n");
            sb.Append("  Primary: ").Append(Primary).Append("\n");
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
        /// <param name="input">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object input)
        {
            return this.Equals(input as BalanceMap);
        }

        /// <summary>
        /// Returns true if BalanceMap instances are equal
        /// </summary>
        /// <param name="input">Instance of BalanceMap to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(BalanceMap input)
        {
            if (input == null)
                return false;

            return base.Equals(input) && 
                (
                    this.Primary == input.Primary ||
                    this.Primary != null &&
                    input.Primary != null &&
                    this.Primary.SequenceEqual(input.Primary)
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hashCode = base.GetHashCode();
                if (this.Primary != null)
                    hashCode = hashCode * 59 + this.Primary.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// To validate all properties of the instance
        /// </summary>
        /// <param name="validationContext">Validation context</param>
        /// <returns>Validation Result</returns>
        IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            yield break;
        }
    }

}
