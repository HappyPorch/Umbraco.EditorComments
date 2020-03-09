using Our.Umbraco.EditorComments.Core.Models;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations;

namespace Our.Umbraco.EditorComments.Core.Migrations
{
    internal class Migration_1_0_0 : MigrationBase
    {
        public Migration_1_0_0(IMigrationContext context) : base(context)
        {
        }

        public override void Migrate()
        {
            Logger.Debug<Migration_1_0_0>("Running migration {MigrationStep}", "Migration_1_0_0");

            // Create the Comments database table
            if (TableExists(Comment.TableName) == false)
            {
                Create.Table<Comment>().Do();
            }
            else
            {
                Logger.Debug<Comment>("The database table {DbTable} already exists, skipping", Comment.TableName);
            }
        }
    }
}
