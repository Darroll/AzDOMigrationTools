using System.Collections.Generic;
using Newtonsoft.Json;
using ADO.RestAPI.Viewmodel50;

namespace ADO.RestAPI.Tasks.Security
{
    public sealed class SecurityTasks
    {
        [JsonProperty(PropertyName = "securityTaskList")]
        public List<SecurityTask> SecurityTaskList { get; private set;  }

        [JsonIgnore]
        public List<SecurityTask> ConfirmedSecurityTaskList { get; private set;  }

        public SecurityTasks()
        {
            // Instantiate.
            ConfirmedSecurityTaskList = new List<SecurityTask>();
            SecurityTaskList = new List<SecurityTask>();
        }

        public void AddTask(string namespaceId, string token, bool Merge, string identityDescriptor, SecurityResponse.AccessControlEntry ace)
        {            
            // Create a new security task.
            SecurityTask task = new SecurityTask()
            {
                Token = token,
                Merge = Merge,
                SecurityNamespaceId = namespaceId,
                AccessControlEntries = new List<SecurityResponse.AccessControlEntry>()
                                        {
                                            new SecurityResponse.AccessControlEntry()
                                            {
                                                Allow = ace.ExtendedInfo.EffectiveAllow,
                                                Deny = ace.ExtendedInfo.EffectiveDeny,
                                                Descriptor = identityDescriptor,
                                                ExtendedInfo = null
                                            }
                                        }
            };

            // Add to list.
            SecurityTaskList.Add(task);
        }

        public void ConfirmTask(SecurityTask task)
        {
            // Add to confirmed list.
            ConfirmedSecurityTaskList.Add(task);
        }

        #region - Nested Classes

        public class SecurityTask
        {
            [JsonProperty(PropertyName = "token")]
            public string Token { get; set; }

            [JsonProperty(PropertyName = "merge")]
            public bool Merge { get; set; }

            [JsonProperty(PropertyName = "accessControlEntries")]
            public List<SecurityResponse.AccessControlEntry> AccessControlEntries { get; set; }

            [JsonProperty(PropertyName = "securityNamespaceId")]
            public string SecurityNamespaceId { get; set; }
        }

        #endregion
    }
}
