﻿using System.Threading.Tasks;
using Frapid.Dashboard.DAL;
using Frapid.Dashboard.DTO;
using Frapid.Dashboard.Hubs;

namespace Frapid.Dashboard.Helpers
{
    public static class NotificationHelper
    {
        public static async Task SendAsync(string tenant, Notification message)
        {
            message.Tenant = tenant;
            await Notifications.AddAsync(tenant, message).ConfigureAwait(false);
            NotificationHub.Send(tenant, message);
        }

        public static async Task SendToAdminsAsync(string tenant, Notification message)
        {
            message.Tenant = tenant;
            await Notifications.AddAsync(tenant, message).ConfigureAwait(false);
            NotificationHub.SendToAdmins(tenant, message);
        }

        public static async Task SendToAdminsAsync(string tenant, int officeId, Notification message)
        {
            message.Tenant = tenant;
            message.OfficeId = officeId;

            await Notifications.AddAsync(tenant, message).ConfigureAwait(false);
            NotificationHub.SendToAdmins(tenant, officeId, message);
        }


        public static async Task SendToRolesAsync(string tenant, int roleId, Notification message)
        {
            message.Tenant = tenant;
            message.ToRoleId = roleId;

            await Notifications.AddAsync(tenant, message).ConfigureAwait(false);
            NotificationHub.SendToRoles(tenant, roleId, message);
        }

        public static async Task SendToRolesAsync(string tenant, int roleId, int officeId, Notification message)
        {
            message.Tenant = tenant;
            message.OfficeId = roleId;
            message.ToRoleId = roleId;

            await Notifications.AddAsync(tenant, message).ConfigureAwait(false);
            NotificationHub.SendToRoles(tenant, roleId, officeId, message);
        }

        public static async Task SendToUsersAsync(string tenant, int userId, Notification message)
        {
            message.Tenant = tenant;
            message.ToUserId = userId;

            await Notifications.AddAsync(tenant, message).ConfigureAwait(false);
            NotificationHub.SendToUsers(tenant, userId, message);
        }

        public static async Task SendToUsersAsync(string tenant, int userId, int officeId, Notification message)
        {
            message.Tenant = tenant;
            message.OfficeId = userId;
            message.ToUserId = userId;

            await Notifications.AddAsync(tenant, message).ConfigureAwait(false);
            NotificationHub.SendToUsers(tenant, userId, officeId, message);
        }

        public static async Task SendToLoginAsync(string tenant, long loginId, Notification message)
        {
            message.Tenant = tenant;
            message.ToLoginId = loginId;

            await Notifications.AddAsync(tenant, message).ConfigureAwait(false);
            NotificationHub.SendToLogin(tenant, loginId, message);
        }
    }
}