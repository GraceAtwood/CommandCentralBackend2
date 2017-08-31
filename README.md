# Command Central Backend 2

This is the new repository for the new version Command Central's Backend, rewritten from the ground up.
 
The Command Central Backend is a RESTful service that provides data access, authentication, validation, and
authorization services to Navy personnel data.  The service includes a few features which we've encapsulated in
"modules":

  - Core
    * Personnel data management (update, search, etc. user profiles)
    * Comprehenisive and extensible permissions system
    * Email system
    * Extensible logging system
    * Windows service support
  - News
    * Update, add, delete, load news items (resembles a blog style system)
  - Muster
    * Take daily accountability of all active users in the data base.
    * Generate reports based on previous days' muster.
    * Automatically turn over muster every day at set times.
  - Correspondence
    * Create digital correspondence.
    * Edit, approve, disapprove correspondence.
    * Attach important files to a correspondence item.
    * Notify all concerned parties of all updates.
    
### The Goal 

> Provide a common access point for personnel data from across the command, creating a more simple user experience for the Sailors, reducing administrative overhead, and opening new avenues of synergy.

### References

Command Central sits atop the shoulders of giants.  Without their work, this project simply would never have happened.  In no particular order:

* [NHibernate] - NHibernate is a mature, open source object-relational mapper for the .NET framework. 
* [FluentNHibnerate] - Fluent, XML-less, compile safe, automated, convention-based mappings for NHibernate.
* [fluent-email] - Though we are no longer using fluent-email, the current email module's design drew heavily on inspriation gained from this project.
* [FluentScheduler] - Automated job scheduler with fluent interface.
* [FluentValidation] - A validation library with a fluent interface.
* [lesi.collections] - Heavily extends the System.Collections namespace.
* [Microsoft.AspNet.Razor] - The runtime render/view engine used by ASP.NET applications.  We use this to render our emails.
* [RazorEngine] - A templating engine built on top of Microsoft's ASP.NET Razor view engine.  We use this to render our emails.
* [MySql.Data] - ADO.NET driver for MySQL.
* [Newtonsoft.Json] - Litterally the best JSON serialization library there is.  This thing can serialize anything and ask for more.
* [NHibernate.Caches.SysCache] - Cache provider for NHibernate which uses the ASP.NET caching engine.
* [Polly] - Allows us to express transient exception handling.  We use this in the Email module to enable the retry behavior on failed sends.
* [ASP.NET Core] - Framework on witch the web part of the web service is built.
* [Swashbuckle] - Makes it easy to generate a swagger interface for documentation and testing.
* [TimePeriodLibrary] - Simplifies dealing with time periods, because that's boring to do yourself.

__*Important Note:*__ We currently are very slightly dependant on the .NET Framework. As soon as [NHibernate] is updated
to [.NET Core] 2.0, we'll be moving completely to that, and you'll be able to develop on *nix as well. Weeeeee!

### Operation

The service may by launched in two modes:
* Interactive
* Windows Service

#### Interactive

Launching the service in interactive mode means little more than executing the service from the command line. 

Make sure you have `appsettings.json` set up properly. There's an example file,
[`appsettings.example.json`](CommandCentral/appsettings.example.json).

```sh
CommandCentral.exe launch
```

### Development

Want to contribute? Great!

__*Important Note:*__ This application is developed with JetBrain's Rider, and a .editorconfig file to keep things
consistent where need be. We don't maintain support for Visual Studio. It shouldn't be too hard to get it working on
Visual Studio, but you're on your own. Just pony up the dough for Rider, and thank us later.

Atwood and McLean are responsible for all changes to the branches master, Pre-Production and Production.  Please feel free to fork or make ask for access to the repository and make your own branch.

Please communicate with the development team to understand the current direction of the project and what we're working on next.

#### Building from source

Clone the repository in Jetbrain's Rider. Rider will suggest that you reacquire all the Nuget packages, and you should
do so if you want it to, like, compile, and stuff.

You'll need a MySql DB to use, and we recommend a program like PaperCut for testing that emails are being sent properly.

Duplicate [`appsettings.example.json`](CommandCentral/appsettings.example.json) as `appsettings.json`, and edit the
settings as appropriate. If you're using a DB on the local machine and the default port, you should only need to edit
the mysql username and password.

If you're on Windows, you'll need to open the port for Command Central to use. To do that, pop open an
elevated/administer command prompt and enter the following, with the same port in `appsettings.json`:

>```netsh http add urlacl url=http://+:1113/ user=Everyone```

Alternatively, you can open Rider as an administrator every time.

Once we've moved completely to .NET Core, and you can develop on Linux, this won't be necessary, because why in the fuck
is that necessary anywhere.

Create two launch configurations, targeting the .NET Framework v4.7:
  1. A 'build db' configuration with the command line arguments "build testdata". This will build the database and
  insert random test data to play with. You can also simply use "build" to build an empty database.
  2. A 'launch' configuration with the command line arguments "launch". Guess what this does.

 
License
----

The Please Don't Sue Us License 2017

[//]: # (These are reference links used in the body of this note and get stripped out when the markdown processor does its job. There is no need to format nicely because it shouldn't be seen. Thanks SO - http://stackoverflow.com/questions/4823468/store-comments-in-markdown-syntax)

[NHibernate]: <http://nhibernate.info/>
[FluentNHibnerate]: <http://www.fluentnhibernate.org/>
[CommandLineParser]: <https://github.com/gsscoder/commandline>
[fluent-email]: <https://github.com/lukencode/FluentEmail>
[FluentScheduler]: <https://github.com/fluentscheduler/FluentScheduler>
[FluentValidation]: <https://github.com/JeremySkinner/FluentValidation>
[lesi.collections]: <https://github.com/nhibernate/iesi.collections>
[Microsoft.AspNet.Razor]: <https://www.nuget.org/packages/microsoft.aspnet.razor/>
[RazorEngine]: <https://github.com/Antaris/RazorEngine>
[MySql.Data]: <http://dev.mysql.com/downloads/connector/net/>
[Newtonsoft.Json]: <https://github.com/JamesNK/Newtonsoft.Json>
[NHibernate.Caches.SysCache]: <https://github.com/diegose/NHibernate.Diegose>
[Polly]: <https://github.com/App-vNext/Polly>
[ASP.NET Core]: <https://github.com/aspnet/Home>
[Swashbuckle]: <https://github.com/domaindrivendev/Swashbuckle>
[TimePeriodLibrary]: <https://github.com/Giannoudis/TimePeriodLibrary>
[.NET Core]: <https://github.com/dotnet/core>
