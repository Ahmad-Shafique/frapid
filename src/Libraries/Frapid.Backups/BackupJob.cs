﻿using System;
using System.Web.Hosting;
using Frapid.Backups.SqlServer;
using Frapid.Configuration;
using Quartz;
using Serilog;

namespace Frapid.Backups
{
    public class BackupJob: IJob
    {
        public async void Execute(IJobExecutionContext context)
        {
            string fileName = DateTimeOffset.UtcNow.Ticks.ToString();
            var domains = TenantConvention.GetDomains();

            foreach(var domain in domains)
            {
                string tenant = TenantConvention.GetDbNameByConvention(domain.DomainName);
                string directory = this.GetBackupDirectory(domain, tenant);


                var server = new DbServer(tenant);
                var agent = this.GetAgent(server, fileName, tenant, directory);

                try
                {
                    await agent.BackupAsync
                        (
                         done =>
                         {
                             var backup = new Resources(tenant, directory, fileName);

                             backup.AddTenantDataToBackup();
                             backup.Compress();
                             backup.Clean();
                         },
                         error =>
                         {
                             Log.Error($"Could not backup because and error occurred. \n\n{error}");
                         });
                }
                catch(Exception ex)
                {
                    Log.Error("Exception occurred executing the backup job. {Exception}.", ex);
                }
            }
        }

        public string GetBackupDirectory(ApprovedDomain domain, string tenant)
        {
            if(domain.BackupDirectoryIsFixedPath &&
               !string.IsNullOrWhiteSpace(domain.BackupDirectory))
            {
                return domain.BackupDirectory;
            }

            if(!string.IsNullOrWhiteSpace(domain.BackupDirectory))
            {
                return HostingEnvironment.MapPath(domain.BackupDirectory);
            }

            string path = $"/Backups/{tenant}/backup";
            return HostingEnvironment.MapPath(path);
        }


        public IDbAgent GetAgent(DbServer server, string backupFileName, string tenant, string backupPath)
        {
            if(server.ProviderName.ToUpperInvariant().Equals("SQL SERVER"))
            {
                return new Agent
                       {
                           Server = server,
                           FileName = backupFileName,
                           Tenant = tenant,
                           BackupFileLocation = backupPath
                       };
            }


            return new Postgres.Agent
                   {
                       Server = server,
                       FileName = backupFileName,
                       Tenant = tenant,
                       BackupFileLocation = backupPath
                   };
        }
    }
}