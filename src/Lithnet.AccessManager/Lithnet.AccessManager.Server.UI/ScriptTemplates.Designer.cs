﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Lithnet.AccessManager.Server.UI {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class ScriptTemplates {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ScriptTemplates() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Lithnet.AccessManager.Server.UI.ScriptTemplates", typeof(ScriptTemplates).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to # Add-DomainGroupMembershipPermissions
        ///# 
        ///# This script adds the Access Manager service account to the &quot;Windows Authorization Access Group&quot; and &quot;Access Control Assistance Operators&quot; groups in the specified domain
        ///#
        ///# This script requires membership in the Domain Admins group for the domain where permissions need to be added
        ///#
        ///# Version 1.0
        ///
        ///
        ///#-------------------------------------------------------------------------
        ///# Do not modify below here
        ///#------------------------------------------------------ [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string AddDomainGroupMembershipPermissions {
            get {
                return ResourceManager.GetString("AddDomainGroupMembershipPermissions", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to function Write-AuditLog{
        ///    param(
        ///    [hashtable]$tokens,
        ///    [bool]$isSuccess
        ///)
        ///    Write-Information &quot;We&apos;re in PowerShell for auditing!&quot;;
        ///
        ///    $user = $tokens[&quot;{user.MsDsPrincipalName}&quot;]
        ///    $computer = $tokens[&quot;{computer.msdsprincipalname}&quot;]
        ///    $result = $tokens[&quot;{AuthzResult.ResponseCode}&quot;]
        ///    $accessType = $tokens[&quot;{AuthzResult.AccessType}&quot;]
        ///
        ///    if ($isSuccess)
        ///    {
        ///        Write-Information &quot;User $user successfully requested $accessType access to $computer&quot;;
        ///    }
        ///    else
        ///    { [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string AuditScriptTemplate {
            get {
                return ResourceManager.GetString("AuditScriptTemplate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to function Get-AuthorizationResponse{
        ///	param(
        ///	$user,
        ///	$computer
        ///)
        ///
        ///	# See https://go.lithnet.io/fwlink/oyotjigz for help
        ///
        ///	Write-Information  &quot;We&apos;re in PowerShell!&quot;
        ///	Write-Information &quot;Checking if $($user.MsDsPrincipalName) is allowed access to $($computer.MsDsPrincipalName)&quot;
        ///
        ///	# Create an object to hold our authorization decisions
        ///	# Set IsAllowed to true to allow access, or set IsDenied to explicitly deny access, or leave both as false if no decision was made. This will allow other rules to be  [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string AuthorizationScriptTemplate {
            get {
                return ResourceManager.GetString("AuthorizationScriptTemplate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to # Create-GroupManagedServiceAccount
        ///# 
        ///# This script enables the KDS service in the domain if it is not already enabled, and creates a new group-managed service account to use with the Access Manager service
        ///#
        ///# This script requires membership in the Domain Admins group
        ///#
        ///# Version 1.0
        ///
        ///#-------------------------------------------------------------------------
        ///# Set the following values as appropriate for your environment
        ///#-------------------------------------------------------------------------
        /// [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string CreateGmsa {
            get {
                return ResourceManager.GetString("CreateGmsa", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to # Enable-PamFeature
        ///# 
        ///# This script enables the &apos;Privileged Access Management&apos; optional feature in a forest
        ///#
        ///# This script requires membership in the Domain Admins group of the forest root domain or Enterprise Admins
        ///#
        ///# Version 1.0
        ///
        ///#-------------------------------------------------------------------------
        ///# Do not modify below here
        ///#-------------------------------------------------------------------------
        ///
        ///Import-Module ActiveDirectory
        ///
        ///$ErrorActionPreference = &quot;Stop&quot;
        ///$InformationPreferen [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string EnablePamFeature {
            get {
                return ResourceManager.GetString("EnablePamFeature", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to # Grant-AccessManagerPermissions
        ///# 
        ///# This script grants permissions for computer objects to write their encrypted password details to the directory, and allows the Lithnet Access Manager service account to read that data
        ///#
        ///# This script requires membership in the Domain Admins group 
        ///#
        ///# Version 1.0
        ///
        ///
        ///#-------------------------------------------------------------------------
        ///# Set the following values as appropriate for your environment
        ///#---------------------------------------------------------- [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GrantAccessManagerPermissions {
            get {
                return ResourceManager.GetString("GrantAccessManagerPermissions", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to # Grant-BitLockerRecoveryPasswordPermissions
        ///# 
        ///# This script grants permissions that allow the Lithnet Admin Access Manager service account to read BitLocker recovery passwords from Active Directory
        ///#
        ///# This script requires membership in the Domain Admins group 
        ///#
        ///# Version 1.0
        ///
        ///
        ///#-------------------------------------------------------------------------
        ///# Set the following values as appropriate for your environment
        ///#-------------------------------------------------------------------------
        ///
        ///# L [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GrantBitLockerRecoveryPasswordPermissions {
            get {
                return ResourceManager.GetString("GrantBitLockerRecoveryPasswordPermissions", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to # Grant-GroupPermissions
        ///# 
        ///# This script grants permissions for the AMS service account to create, delete, and manage groups in the specified OU
        ///#
        ///# This script requires membership in the Domain Admins group 
        ///# 
        ///#
        ///# Version 1.0
        ///
        ///#-------------------------------------------------------------------------
        ///# Do not modify below here
        ///#-------------------------------------------------------------------------
        ///
        ///Import-Module ActiveDirectory
        ///
        ///$ErrorActionPreference = &quot;Stop&quot;
        ///$InformationPreference =  [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GrantGroupPermissions {
            get {
                return ResourceManager.GetString("GrantGroupPermissions", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to # Grant-MsLapsPermissions
        ///# 
        ///# This script grants permissions that allow the Lithnet Admin Access Manager service account to read the Microsoft LAPS passwords from Active Directory
        ///#
        ///# This script requires membership in the Domain Admins group 
        ///#
        ///# Version 1.0
        ///
        ///
        ///#-------------------------------------------------------------------------
        ///# Set the following values as appropriate for your environment
        ///#-------------------------------------------------------------------------
        ///
        ///# Leave this value bla [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GrantMsLapsPermissions {
            get {
                return ResourceManager.GetString("GrantMsLapsPermissions", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to # Prevent-Delegation
        ///# 
        ///# This script modifies the userAccountControl attribute to set the flag that prevents delegation of the specified account
        ///#
        ///# This script requires membership in the domain admins group, or delegated permission to manage user objects in the container where the account resides
        ///# 
        ///#
        ///# Version 1.0
        ///
        ///$ErrorActionPreference = &quot;Stop&quot;
        ///$InformationPreference = &quot;Continue&quot;
        ///
        ///Import-Module ActiveDirectory
        ///    
        ///$sid = &quot;{sid}&quot;            
        ///Set-ADAccountControl -Identity $sid -AccountNo [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string PreventDelegation {
            get {
                return ResourceManager.GetString("PreventDelegation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to # Publish-LithnetAccessManagerCertificate
        ///# 
        ///# This script creates an object in the Configuration Naming context of the root domain in the forest with a copy
        ///# that contains the public key of the certificate Lithnet Access Manager Agents should use to encrypt their local
        ///# admin passwords and password history
        ///#
        ///# This script requires membership in their the Enterprise Admin group, or the Domain Admin group on the root domain of the forest
        ///# 
        ///# Note, this script has been pre-populated with the inform [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string PublishCertificateTemplate {
            get {
                return ResourceManager.GetString("PublishCertificateTemplate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to # Update-AdSchema
        ///# 
        ///# This script creates the attributes and object classes required to enable encrypted local admin passwords and password history support
        ///# with the Lithnet Access Manager Agent
        ///#
        ///# This script requires membership in their the Schema Admin group
        ///# 
        ///#
        ///# Version 1.0
        ///
        ///#-------------------------------------------------------------------------
        ///# Do not modify below here
        ///#-------------------------------------------------------------------------
        ///
        ///$ErrorActionPreference = &quot;Stop&quot;
        ///$I [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string UpdateAdSchemaTemplate {
            get {
                return ResourceManager.GetString("UpdateAdSchemaTemplate", resourceCulture);
            }
        }
    }
}
