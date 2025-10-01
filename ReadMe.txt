
1. Add-Migration InitialCreate -Project Uptime.Workflows.Core -StartupProject Uptime.Workflows.API -OutputDir Data/Migrations
   - Update-Database 0 -Project Uptime.Workflows.Core -StartupProject Uptime.Workflows.API
2. Remove-Migration -Project Uptime.Workflows.Core -StartupProject Uptime.Workflows.API
3. Update-Database -Project Uptime.Workflows.Core -StartupProject Uptime.Workflows.API
4. Drop-Database -Project Uptime.Workflows.Core -StartupProject Uptime.Workflows.API

	- Project Uptime.Workflows.Core → Ensures migrations are added to the Core project.
	- StartupProject Uptime.API → Ensures EF Core loads the connection string from appsettings.json.


Workflows:

I. Each workflow must have:
	1. workflow definition implementation
	2. state machine configuration implementation

II. Workflow context is a data object that contains all the data needed to run a workflow after reHydration.
	Context data is stored into database.

III. The services for communicating with the workflow data layer are implemented in the core library but are replaceable.