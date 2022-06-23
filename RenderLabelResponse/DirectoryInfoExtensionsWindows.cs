namespace RenderExpressConnectResponse
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Versioning;
    using System.Security.AccessControl;
    using System.Security.Principal;

    [SupportedOSPlatform("windows")]
    internal static class DirectoryInfoExtensionsWindows
    {
        public static bool CheckWriteAccess(this DirectoryInfo dir)
        {
            // ***** check windows user *****
            WindowsIdentity wid = WindowsIdentity.GetCurrent();

            bool denied = false;
            bool allowed = false;

            // ***** check write access *****
            try
            {
                DirectorySecurity acl = dir.GetAccessControl();
                AuthorizationRuleCollection arc = acl.GetAccessRules(true, true, typeof(SecurityIdentifier));
                IList<FileSystemAccessRule> ars = new List<FileSystemAccessRule>(arc.OfType<FileSystemAccessRule>());

                IList<IdentityReference> widgs = wid.Groups.Where(g => ((SecurityIdentifier)g).IsAccountSid()).ToList();

                // ***** user, not inherited rules *****
                foreach (FileSystemAccessRule rule in ars.Where(r => r.IdentityReference.Equals(wid.User) && !r.IsInherited))
                {
                    denied |= DeniesWriteAccess(rule);
                    allowed |= AllowsWriteAccess(rule);
                }

                // ***** user, inherited rules *****
                foreach (FileSystemAccessRule rule in ars.Where(r => r.IdentityReference.Equals(wid.User) && r.IsInherited))
                {
                    denied |= DeniesWriteAccess(rule);
                    allowed |= AllowsWriteAccess(rule);
                }

                // ***** groups, not inherited rules *****
                foreach (FileSystemAccessRule rule in ars.Where(r => widgs.Contains(r.IdentityReference) && !r.IsInherited))
                {
                    denied |= DeniesWriteAccess(rule);
                    allowed |= AllowsWriteAccess(rule);
                }

                // ***** groups, inherited rules *****
                foreach (FileSystemAccessRule rule in ars.Where(r => widgs.Contains(r.IdentityReference) && r.IsInherited))
                {
                    denied |= DeniesWriteAccess(rule);
                    allowed |= AllowsWriteAccess(rule);
                }
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }

            return !denied && allowed;
        }

        private static bool AllowsWriteAccess(FileSystemAccessRule rule) => rule.AccessControlType == AccessControlType.Allow
            && (
            rule.FileSystemRights.HasFlag(FileSystemRights.Write)
            || rule.FileSystemRights.HasFlag(FileSystemRights.WriteData)
            || rule.FileSystemRights.HasFlag(FileSystemRights.CreateDirectories)
            || rule.FileSystemRights.HasFlag(FileSystemRights.CreateFiles)
            );

        private static bool DeniesWriteAccess(FileSystemAccessRule rule) => rule.AccessControlType == AccessControlType.Deny
            && (
            rule.FileSystemRights.HasFlag(FileSystemRights.Write)
            || rule.FileSystemRights.HasFlag(FileSystemRights.WriteData)
            || rule.FileSystemRights.HasFlag(FileSystemRights.CreateDirectories)
            || rule.FileSystemRights.HasFlag(FileSystemRights.CreateFiles)
            );
    }
}
