
1. Add-Migration InitialCreate -Project Uptime.Persistence -StartupProject Uptime.WorkflowAPI
2. Remove-Migration -Project Uptime.Persistence -StartupProject Uptime.WorkflowAPI
3. Update-Database -Project Uptime.Persistence -StartupProject Uptime.WorkflowAPI
4. Drop-Database -Project Uptime.Persistence -StartupProject Uptime.WorkflowAPI

	- Project Uptime.Persistence → Ensures migrations are added to the Persistence project.
	- StartupProject Uptime.API → Ensures EF Core loads the connection string from appsettings.json.
