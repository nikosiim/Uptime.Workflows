
1. Add-Migration InitialCreate -Project Uptime.Domain -StartupProject Uptime.WorkflowAPI -OutputDir Data/Migrations
2. Remove-Migration -Project Uptime.Domain -StartupProject Uptime.WorkflowAPI
3. Update-Database -Project Uptime.Domain -StartupProject Uptime.WorkflowAPI
4. Drop-Database -Project Uptime.Domain -StartupProject Uptime.WorkflowAPI

	- Project Uptime.Domain → Ensures migrations are added to the Domain project.
	- StartupProject Uptime.API → Ensures EF Core loads the connection string from appsettings.json.


Küsimused:
----------
1. Kuidas implementeerida ülesannete ringid?
2. Kas tööülesande sisendi peaks saatma töövoogu ikka Dictionary objektiga 
või hoopis json-na, mis vastavas töövoos cast-ks õigeks objektiks
3. TaskData tüüp on objekt ja see salvestatakse baasi JsonElement objektina, kas see on ok nii jätta?.