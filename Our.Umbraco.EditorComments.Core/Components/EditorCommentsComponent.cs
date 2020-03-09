using Our.Umbraco.EditorComments.Core.Controllers.Api;
using Our.Umbraco.EditorComments.Core.Migrations;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations;
using Umbraco.Core.Migrations.Upgrade;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.JavaScript;

namespace Our.Umbraco.EditorComments.Core.Components
{
    public class EditorCommentsComponent : IComponent
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly IMigrationBuilder _migrationBuilder;
        private readonly IKeyValueService _keyValueService;
        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EditorCommentsComponent(IScopeProvider scopeProvider, IMigrationBuilder migrationBuilder, IKeyValueService keyValueService, ILogger logger, IHttpContextAccessor httpContextAccessor)
        {
            _scopeProvider = scopeProvider;
            _migrationBuilder = migrationBuilder;
            _keyValueService = keyValueService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public void Initialize()
        {
            ExecuteMigrations();
            SetupServerVariables();
        }

        public void Terminate()
        {
        }

        private const string MigrationPlanName = "EditorComments";

        private void ExecuteMigrations()
        {
            // create migration plan for the plugin
            var migrationPlan = new MigrationPlan(MigrationPlanName);

            // add migrations to plan
            migrationPlan.From(string.Empty)
                .To<Migration_1_0_0>("1.0.0");

            // run migrations against the database
            var upgrader = new Upgrader(migrationPlan);

            upgrader.Execute(_scopeProvider, _migrationBuilder, _keyValueService, _logger);
        }

        private void SetupServerVariables()
        {
            ServerVariablesParser.Parsing += (sender, serverVars) =>
            {
                // add API base URL to the server variables for use in JS
                if (!serverVars.ContainsKey("umbracoUrls"))
                    throw new ArgumentException("Missing umbracoUrls.");
                var umbracoUrlsObject = serverVars["umbracoUrls"];
                if (umbracoUrlsObject == null)
                    throw new ArgumentException("Null umbracoUrls");
                if (!(umbracoUrlsObject is Dictionary<string, object> umbracoUrls))
                    throw new ArgumentException("Invalid umbracoUrls");

                var urlHelper = new UrlHelper(_httpContextAccessor.HttpContext.Request.RequestContext);

                umbracoUrls["editorCommentsBaseUrl"] = urlHelper.GetUmbracoApiServiceBaseUrl<EditorCommentsController>(controller => controller.GetComments(0, null));
            };
        }
    }
}
