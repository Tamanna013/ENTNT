using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FleetMind.Api.Common.Constants;

namespace FleetMind.Api.Data.Seed
{
    public static class RoleSeedData
    {
        public static List<string> GetAllRoleNames()
        {
            // Use reflection to get all public constant strings in AppRoles
            var roleFields = typeof(AppRoles).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string));

            var roles = new List<string>();
            foreach (var field in roleFields)
            {
                var roleName = field.GetRawConstantValue()?.ToString();
                if (!string.IsNullOrEmpty(roleName))
                {
                    roles.Add(roleName);
                }
            }

            return roles;
        }
    }
}
