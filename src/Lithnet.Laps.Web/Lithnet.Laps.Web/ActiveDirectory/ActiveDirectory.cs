﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Security.Principal;
using System.Text;
using Lithnet.Laps.Web.ActiveDirectory.Interop;
using Lithnet.Laps.Web.Internal;
using Lithnet.Laps.Web.Models;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography.X509Certificates;

namespace Lithnet.Laps.Web.ActiveDirectory
{
    public sealed class ActiveDirectory : IDirectory
    {
        private static Guid PamFeatureGuid = new Guid("ec43e873-cce8-4640-b4ab-07ffe4ab5bcd");

        private Dictionary<SecurityIdentifier, bool> PamEnabledDomainCache = new Dictionary<SecurityIdentifier, bool>();

        public IUser GetUser(string userName)
        {
            SearchResult user = this.DoGcLookup(userName, "user", ActiveDirectoryUser.PropertiesToGet);
            return user == null ? null : new ActiveDirectoryUser(user);
        }

        public IComputer GetComputer(string computerName)
        {
            SearchResult result = this.DoGcLookup(computerName, "computer", ActiveDirectoryComputer.PropertiesToGet);

            return result == null ? null : new ActiveDirectoryComputer(result);
        }

        public ISecurityPrincipal GetPrincipal(string principalName)
        {
            SearchResult result = this.DoGcLookup(principalName, "*", ActiveDirectoryComputer.PropertiesToGet);

            if (result.HasPropertyValue("objectClass", "computer"))
            {
                return new ActiveDirectoryComputer(result);
            }

            if (result.HasPropertyValue("objectClass", "group"))
            {
                return new ActiveDirectoryGroup(result);
            }

            if (result.HasPropertyValue("objectClass", "user"))
            {
                return new ActiveDirectoryUser(result);
            }

            throw new UnsupportedPrincipalTypeException($"The object '{principalName}' was of an unknown type: {result.GetPropertyCommaSeparatedString("objectClass")}");
        }

        public bool IsContainer(string path)
        {
            try
            {
                SearchResult result = this.GetDirectoryEntry(path, "*", "objectClass");

                return result.HasPropertyValue("objectClass", "organizationalUnit") ||
                       result.HasPropertyValue("objectClass", "domain") ||
                       result.HasPropertyValue("objectClass", "domainDNS") ||
                       result.HasPropertyValue("objectClass", "container");
            }
            catch
            {
                return false;
            }
        }

        public bool IsComputerInOu(IComputer computer, string ou)
        {
            DirectorySearcher d = new DirectorySearcher
            {
                SearchRoot = new DirectoryEntry($"GC://{ou}"),
                SearchScope = SearchScope.Subtree,
                Filter = $"objectGuid={computer.Guid.ToOctetString()}"
            };

            return d.FindOne() != null;
        }

        public IGroup GetGroup(string groupName)
        {
            SearchResult result = this.DoGcLookup(groupName, "group", ActiveDirectoryGroup.PropertiesToGet);
            return result == null ? null : new ActiveDirectoryGroup(result);
        }

        public IGroup GetGroup(SecurityIdentifier sid)
        {
            SearchResult result = this.DoGcLookup(sid.ToString(), "group", ActiveDirectoryGroup.PropertiesToGet);
            return result == null ? null : new ActiveDirectoryGroup(result);
        }

        public bool IsSidInPrincipalToken(SecurityIdentifier sidToFindInToken, ISecurityPrincipal principal)
        {
            return this.IsSidInPrincipalToken(sidToFindInToken, principal, principal.Sid.AccountDomainSid);
        }

        public bool IsSidInPrincipalToken(SecurityIdentifier sidToFindInToken, ISecurityPrincipal principal, SecurityIdentifier targetDomainSid)
        {
            return NativeMethods.CheckForSidInToken(principal.Sid, sidToFindInToken, targetDomainSid);
        }

        public IComputer GetComputer()
        {
            return this.GetComputer(this.GetMachineNetbiosFullyQualifiedName());
        }

        public string GetMachineNetbiosDomainName()
        {
            var result = NativeMethods.GetWorkstationInfo(null);
            return result.LanGroup;
        }

        public string GetMachineNetbiosFullyQualifiedName()
        {
            var result = NativeMethods.GetWorkstationInfo(null);
            return $"{result.LanGroup}\\{result.ComputerName}";
        }

        //public IList<ISecurityPrincipal> GetGroupMembers(IGroup group)
        //{
        //    //var groupEntry =
        //    throw new NotImplementedException();
        //}

        //public IEnumerable<string> GetNestedMemberDNsFromGroup(IGroup group)
        //{
        //    HashSet<string> memberDNs = new HashSet<string>();
        //    this.GetNestedMemberDNsFromGroup(group.DistinguishedName, memberDNs);

        //    return memberDNs;
        //}

        //private void GetNestedMemberDNsFromGroup(string dn, HashSet<string> members)
        //{
        //    foreach (string member in this.GetMemberDNsFromGroup(dn))
        //    {
        //        if (members.Add(member))
        //        {
        //            this.GetNestedMemberDNsFromGroup(member, members);
        //        }
        //    }
        //}

        public IEnumerable<string> GetMemberDNsFromGroup(IGroup group)
        {
            return this.GetMemberDNsFromGroup(group.DistinguishedName);
        }
        private IEnumerable<string> GetMemberDNsFromGroup(string dn)
        {
            HashSet<string> memberDNs = new HashSet<string>();

            int rangeLower = 0;
            int rangeUpper = 1499;
            int rangeStep = 1500;

            while (true)
            {
                var de = this.GetDirectoryEntry(dn, "group", $"member;range={rangeLower}-{rangeUpper}");

                if (de == null)
                {
                    return memberDNs;
                }

                var returnedMemberPropertyName = de.Properties.PropertyNames.OfType<string>().FirstOrDefault(t => t.StartsWith("member;range=", StringComparison.OrdinalIgnoreCase));

                if (returnedMemberPropertyName == null)
                {
                    return memberDNs;
                }

                foreach (var item in de.Properties[returnedMemberPropertyName].OfType<string>())
                {
                    memberDNs.Add(item);
                }

                if (returnedMemberPropertyName.EndsWith("*"))
                {
                    return memberDNs;
                }

                rangeLower = rangeUpper + 1;
                rangeUpper += rangeStep;
            }
        }

        public void AddGroupMember(IGroup group, ISecurityPrincipal principal, TimeSpan ttl)
        {
            var groupEntry = new DirectoryEntry($"LDAP://{group.DistinguishedName}");

            groupEntry.Properties["member"].Add($"<TTL={ttl.TotalSeconds},<SID={principal.Sid}>>");
            groupEntry.CommitChanges();
        }

        public void AddGroupMember(IGroup group, ISecurityPrincipal principal)
        {
            var groupEntry = new DirectoryEntry($"LDAP://{group.DistinguishedName}");

            groupEntry.Properties["member"].Add($"<SID={principal.Sid}>");
            groupEntry.CommitChanges();
        }

        public void CreateTtlGroup(string accountName, string displayName, string description, string ou, TimeSpan ttl)
        {
            DirectoryEntry container = new DirectoryEntry($"LDAP://{ou}");
            dynamic[] objectClasses = new dynamic[] { "dynamicObject", "group" };

            DirectoryEntry group = container.Children.Add($"CN={accountName}", "group");

            group.Invoke("Put", "objectClass", objectClasses);
            group.Properties["samAccountName"].Add(accountName);
            group.Properties["displayName"].Add(displayName);
            group.Properties["description"].Add(description);
            group.Properties["groupType"].Add(-2147483644);
            group.Properties["entryTTL"].Add((int)ttl.TotalSeconds);
            group.CommitChanges();
        }

        public void UpdateComputerLamAppData(SecurityIdentifier groupSid)
        {
            IComputer computer = this.GetComputer();
            DirectoryEntry de;
            IGroup group = this.GetGroup(groupSid);

            try
            {
                string dn = GetJitAccessGroupDn(computer);
                de = new DirectoryEntry(dn);
                var referencedObject = de.Properties["msDS-ObjectReference"].Value as string;
                if (!string.Equals(referencedObject, group.DistinguishedName))
                {
                    de.Properties["msDS-ObjectReference"].Clear();
                    de.Properties["msDS-ObjectReference"].Add(group.DistinguishedName);
                }
            }
            catch (ObjectNotFoundException)
            {
                var parent = new DirectoryEntry($"LDAP://{computer.DistinguishedName}");

                de = parent.Children.Add("CN=LithnetAccessManagerConfig", "msDs-App-Configuration");
                de.Properties["applicationName"].Add("Lithnet Access Manager");
                de.Properties["description"].Add("Application configuration for Lithnet Access Manager");
                de.Properties["msDS-ObjectReference"].Add(group.DistinguishedName);
                de.Properties["ntSecurityDescriptor"].Add(this.GetDefaultSecurityDescriptorForLamObject());
                de.CommitChanges();
            }
        }

        public void GetComputerLamAppData(IComputer computer)
        {
            //DirectoryEntry de;

            //try
            //{
            //    string dn = GetJitAccessGroupDn(computer);
            //    de = new DirectoryEntry(dn);
            //    var referencedObject = de.Properties["msDS-ObjectReference"].Value as string;
            //    if (!string.Equals(referencedObject, group.DistinguishedName))
            //    {
            //        de.Properties["msDS-ObjectReference"].Clear();
            //        de.Properties["msDS-ObjectReference"].Add(group.DistinguishedName);
            //    }
            //}
            //catch (ObjectNotFoundException)
            //{
            //    var parent = new DirectoryEntry($"LDAP://{computer.DistinguishedName}");

            //    de = parent.Children.Add("CN=LithnetAccessManagerConfig", "msDs-App-Configuration");
            //    de.Properties["applicationName"].Add("Lithnet Access Manager");
            //    de.Properties["description"].Add("Application configuration for Lithnet Access Manager");
            //    de.Properties["msDS-ObjectReference"].Add(group.DistinguishedName);
            //    de.Properties["ntSecurityDescriptor"].Add(this.GetDefaultSecurityDescriptorForLamObject());
            //    de.CommitChanges();
            //}
        }

        private byte[] GetDefaultSecurityDescriptorForLamObject()
        {
            ActiveDirectorySecurity gf = new ActiveDirectorySecurity();
            gf.SetSecurityDescriptorSddlForm("D:AI(A;;FA;;;CO)");
            return gf.GetSecurityDescriptorBinaryForm();
        }

        public IGroup GetJitAccessGroup(IComputer computer)
        {
            string dn = GetJitAccessGroupDn(computer);

            if (string.IsNullOrWhiteSpace(dn))
            {
                throw new ObjectNotFoundException($"The JIT access group for the computer {computer.MsDsPrincipalName} was not found");
            }

            var result = GetDirectoryEntry(dn, "group", ActiveDirectoryGroup.PropertiesToGet);

            if (result == null)
            {
                throw new ObjectNotFoundException($"The JIT access group for the computer {computer.MsDsPrincipalName} was not found");
            }

            return new ActiveDirectoryGroup(result);
        }

        public string GetJitAccessGroupDn(IComputer computer)
        {
            DirectorySearcher d = new DirectorySearcher
            {
                SearchRoot = new DirectoryEntry($"LDAP://{computer.DistinguishedName}"),
                SearchScope = SearchScope.OneLevel,
                Filter = $"(&(objectClass=msDs-App-Configuration)(applicationName=Lithnet Access Manager))"
            };

            d.PropertiesToLoad.Add("msDS-ObjectReference");

            var result = d.FindOne();

            if (result == null)
            {
                throw new ObjectNotFoundException();
            }

            return result.GetPropertyString("msDS-ObjectReference");
        }

        private SearchResult GetLamObjject(IComputer computer)
        {
            DirectorySearcher d = new DirectorySearcher
            {
                SearchRoot = new DirectoryEntry($"LDAP://{computer.DistinguishedName}"),
                SearchScope = SearchScope.OneLevel,
                Filter = $"(&(objectClass=msDs-App-Configuration)(applicationName=Lithnet Access Manager))"
            };

            d.PropertiesToLoad.Add("msDS-ObjectReference");

            var result = d.FindOne();

            if (result == null)
            {
                throw new ObjectNotFoundException();
            }

            return result;
        }

        public bool IsPamFeatureEnabled(SecurityIdentifier domainSid)
        {
            SecurityIdentifier sid = domainSid.AccountDomainSid;

            if (PamEnabledDomainCache.TryGetValue(sid, out bool value))
            {
                return value;
            }

            string dc = NativeMethods.GetDnsDomainNameFromSid(sid);

            var rootDse = new DirectoryEntry($"LDAP://{dc}/rootDSE");

            var configNamingContext = (string)rootDse.Properties["configurationNamingContext"]?.Value;

            if (configNamingContext == null)
            {
                throw new ObjectNotFoundException($"Configuration naming context lookup failed");
            }

            DirectorySearcher d = new DirectorySearcher
            {
                SearchRoot = new DirectoryEntry($"LDAP://{configNamingContext}"),
                SearchScope = SearchScope.Subtree,
                Filter = $"(&(objectClass=msDS-OptionalFeature)(msDS-OptionalFeatureGUID={PamFeatureGuid.ToOctetString()}))",
            };

            bool result = d.FindOne() != null;

            PamEnabledDomainCache.Add(domainSid, result);

            return result;
        }

        public bool IsDomainController()
        {
            var info = NativeMethods.GetServerInfo(null);

            return (info.Type & ServerTypes.DomainCtrl) == ServerTypes.DomainCtrl || (info.Type & ServerTypes.BackupDomainCtrl) == ServerTypes.BackupDomainCtrl;
        }

        private SearchResult DoGcLookup(string objectName, string objectClass, IEnumerable<string> propertiesToGet)
        {
            string dn;

            if (objectClass.Equals("computer", StringComparison.OrdinalIgnoreCase) && !objectName.EndsWith("$"))
            {
                objectName += "$";
            }

            if (objectName.Contains("\\") || objectName.Contains("@"))
            {
                dn = NativeMethods.GetDn(objectName);
            }
            else if (objectName.TryParseAsSid(out SecurityIdentifier sid))
            {
                dn = $"<SID={sid}>";
            }
            else if (this.IsDistinguishedName(objectName))
            {
                dn = objectName;
            }
            else
            {
                dn = ActiveDirectory.DoGcLookupFromSimpleName(objectName, objectClass);
            }

            if (dn == null)
            {
                throw new ObjectNotFoundException($"An object {objectName} of type {objectClass} was not found in the global catalog");
            }

            DirectorySearcher d = new DirectorySearcher
            {
                SearchRoot = new DirectoryEntry($"LDAP://{dn}"),
                SearchScope = SearchScope.Base,
                Filter = $"(objectClass={objectClass})"
            };

            foreach (string prop in propertiesToGet)
            {
                d.PropertiesToLoad.Add(prop);
            }

            d.PropertiesToLoad.AddIfMissing("objectClass", StringComparer.OrdinalIgnoreCase);

            SearchResult result = d.FindOne();

            if (result == null)
            {
                throw new ObjectNotFoundException($"The object {dn} was not found in the directory or was not of the object class {objectClass}");
            }

            return result;
        }

        private static string DoGcLookupFromSimpleName(string name, string objectClass)
        {
            DirectorySearcher d = new DirectorySearcher
            {
                SearchRoot = new DirectoryEntry($"GC://{Forest.GetCurrentForest().Name}"),
                SearchScope = SearchScope.Subtree,
                Filter = $"(&(objectClass={objectClass})(samAccountName={ActiveDirectory.EscapeSearchFilterParameter(name)}))"
            };

            d.PropertiesToLoad.Add("distinguishedName");

            SearchResultCollection result = d.FindAll();

            if (result.Count > 1)
            {
                throw new AmbiguousNameException($"There was more than one value in the directory that matched the name {name}");
            }

            if (result.Count == 0)
            {
                return null;
            }

            return result[0].Properties["distinguishedName"][0].ToString();
        }

        public SearchResult GetDirectoryEntry(ISecurityPrincipal principal, params string[] propertiesToLoad)
        {
            return this.GetDirectoryEntry(principal.DistinguishedName, "*", propertiesToLoad);
        }

        public SearchResult SearchDirectoryEntry(string basedn, string filter, SearchScope scope, params string[] propertiesToLoad)
        {
            DirectorySearcher d = new DirectorySearcher
            {
                SearchRoot = new DirectoryEntry($"LDAP://{basedn}"),
                SearchScope = scope,
                Filter = filter
            };

            foreach (string prop in propertiesToLoad)
            {
                d.PropertiesToLoad.Add(prop);
            }

            return d.FindOne();
        }

        public SearchResult GetDirectoryEntry(string dn, string objectClass, params string[] propertiesToLoad)
        {
            return this.SearchDirectoryEntry(dn, $"objectClass={objectClass}", SearchScope.Base, propertiesToLoad);
        }

        private static string EscapeSearchFilterParameter(string p)
        {
            StringBuilder escapedValue = new StringBuilder();

            foreach (char c in p)
            {
                switch (c)
                {
                    case '\\':
                        escapedValue.Append("\\5c");
                        break;

                    case '*':
                        escapedValue.Append("\\2a");
                        break;

                    case '(':
                        escapedValue.Append("\\28");
                        break;

                    case ')':
                        escapedValue.Append("\\29");
                        break;

                    case '\0':
                        escapedValue.Append("\\00");
                        break;

                    default:
                        escapedValue.Append(c);
                        break;
                }
            }

            return escapedValue.ToString();
        }

        private bool IsDistinguishedName(string name)
        {
            if (!name.Contains('='))
            {
                return false;
            }

            try
            {
                X500DistinguishedName d2 = new X500DistinguishedName(name);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}