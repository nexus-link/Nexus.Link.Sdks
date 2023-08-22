
using System.IO;
using System.Linq;
using Dapper;
using Microsoft.Data.SqlClient;
using Nexus.Link.Libraries.SqlServer.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql;
using WorkflowEngine.Sdk.Persistence.Sql.IntegrationTests.Support;
using Xunit;

namespace WorkflowEngine.Sdk.Persistence.Sql.IntegrationTests
{
    [Collection("workflow-sdk-tests")]
    public class RollbackTest : AbstractDatabaseTest
    {

        private const string ConnectionStringForRollback = "Server=localhost;Database=workflow-sdk-tests-rollback;Trusted_Connection=True;Encrypt=False;";

        public class DatabaseRollbackTest
        {
            private readonly DirectoryInfo _baseDir;
            private readonly DirectoryInfo _patchesDir;
            private readonly DirectoryInfo _onceDir;
            private readonly DirectoryInfo _alwaysDir;

            private readonly string _extraPatch1;
            private readonly string _extraPatch1Rollback;
            private readonly string _extraPatch2;
            private readonly string _extraPatch2Rollback;

            static DatabaseRollbackTest()
            {
                DropDatabase(ConnectionStringForRollback);
            }

            public DatabaseRollbackTest()
            {
                _baseDir = DatabasePatcherHandler.GetBaseDir();

                _patchesDir = new DirectoryInfo($@"{_baseDir.FullName}\patches");
                _onceDir = new DirectoryInfo($@"{_baseDir.FullName}\once");
                _alwaysDir = new DirectoryInfo($@"{_baseDir.FullName}\always");
                _extraPatch1 = $@"{_patchesDir.FullName}\extra-patch-1.sql";
                _extraPatch1Rollback = $@"{_patchesDir.FullName}\extra-patch-1-rollback.sql";
                _extraPatch2 = $@"{_patchesDir.FullName}\extra-patch-2.sql";
                _extraPatch2Rollback = $@"{_patchesDir.FullName}\extra-patch-2-rollback.sql";

                // TODO: andra blir påverkade av att det finns nya patch-filer
            }

            [Fact]
            public void VerifyRollbacksForExistingPatches()
            {
                // Patch to latest level
                ClearCacheAndPatch();

                var backupDir = new DirectoryInfo(@"backedup-sql-scripts");
                try
                {
                    using var connection = new SqlConnection(ConnectionStringForRollback);
                    connection.VerifyAvailability();

                    // Current patch level
                    var patchLevel = connection.QuerySingle<long>("SELECT MAX(Version) FROM DbVersion");


                    // Copy patch files to new dir
                    if (!backupDir.Exists)
                    {
                        backupDir.Create();
                        backupDir.Refresh();
                    }
                    CopyDirectory(_baseDir.FullName, backupDir.FullName);

                    // Don't run seed scripts
                    if (_onceDir.Exists) _onceDir.Delete(true);
                    if (_alwaysDir.Exists) _alwaysDir.Delete(true);

                    for (var level = patchLevel; level > 1; level--)
                    {
                        //  Remove PATCH and ROLLBACK files for this patch
                        foreach (var file in Directory.GetFiles(_patchesDir.FullName, "*.*", SearchOption.AllDirectories))
                        {
                            var contents = File.ReadLines(file);
                            var header = contents.FirstOrDefault();
                            if (header == null) continue;
                            if (header.Contains($"PATCH: {level}") || header.Contains($"ROLLBACK: {level}"))
                            {
                                new FileInfo(file).Delete();
                            }
                        }

                        connection.Execute("DELETE FROM SeedOnce");
                        ClearCacheAndPatch();
                    }

                    connection.Close();
                }
                finally
                {
                    if (backupDir.Exists)
                    {
                        CopyDirectory(backupDir.FullName, _baseDir.FullName);
                        backupDir.Delete(true);
                    }
                    ClearCacheAndPatch();
                }
            }

            private static void CopyDirectory(string sourcePath, string destinationPath)
            {
                foreach (var dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dirPath.Replace(sourcePath, destinationPath));
                }

                foreach (var newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                {
                    File.Copy(newPath, newPath.Replace(sourcePath, destinationPath), true);
                }
            }


            [Fact]
            public void VerifyRollbackMechanismForNewPatches()
            {
                // Patch to latest level
                ClearCacheAndPatch();

                try
                {
                    lock (RollbackLock)
                    {
                        using var connection = new SqlConnection(ConnectionStringForRollback);
                        connection.VerifyAvailability();

                        // Current patch level
                        var patchLevel = connection.QuerySingle<long>("SELECT MAX(Version) FROM DbVersion");

                        // Add unit test patch 1
                        WriteExtraPatch1Files(patchLevel);
                        ClearCacheAndPatch();
                        var name = connection.QuerySingle<string>("SELECT Name From UnitTest_Person WHERE Id = 1");
                        Assert.Equal("Adam Andersson", name);

                        // Add unit test patch 2
                        WriteExtraPatch2Files(patchLevel);
                        ClearCacheAndPatch();
                        var firstName =
                            connection.QuerySingle<string>("SELECT FirstName From UnitTest_Person WHERE Id = 1");
                        Assert.Equal("Adam", firstName);

                        // Rollback unit test patch 2
                        DeleteExtraPatch2Files();
                        ClearCacheAndPatch();
                        name = connection.QuerySingle<string>("SELECT Name From UnitTest_Person WHERE Id = 1");
                        Assert.Equal("Adam Andersson", name);

                        // Rollback unit test patch 1
                        DeleteExtraPatch1Files();
                        ClearCacheAndPatch();
                        var result = connection.Query("SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE Table_Name = 'UnitTest_Person'");
                        Assert.Empty(result);

                        connection.Close();

                    }
                }
                finally
                {
                    foreach (var file in _patchesDir.EnumerateFiles("extra-patch-*"))
                    {
                        file.Delete();
                    }
                }
            }

            private static void ClearCacheAndPatch()
            {
                //DatabasePatcherHandler.ClearCache();

                var handler = new DatabasePatcherHandler(ConnectionStringForRollback, PersistenceHelper.MasterConnectionString);
                handler.PatchOrThrowAsync();
            }

            private void WriteExtraPatch1Files(long initialPatchLevel)
            {
                var newPatchLevel = initialPatchLevel + 1;

                File.WriteAllText(_extraPatch1,
                    $"-- PATCH: {newPatchLevel}\n" +
                    "" +
                    "CREATE TABLE UnitTest_Person (Id INTEGER PRIMARY KEY, Name NVARCHAR(255) NOT NULL);\n" +
                    "INSERT INTO UnitTest_Person (Id, Name) VALUES" +
                    " (1, 'Adam Andersson'), (2, 'Berit Bugle'), (3, 'Örjan Öberg');\n"
                );
                File.WriteAllText(_extraPatch1Rollback,
                    $"-- ROLLBACK: {newPatchLevel}\n" +
                    "" +
                    "DROP TABLE UnitTest_Person;\n"
                );
            }

            private void WriteExtraPatch2Files(long initialPatchLevel)
            {
                var newPatchLevel = initialPatchLevel + 2;

                File.WriteAllText(_extraPatch2,
                    $"-- PATCH: {newPatchLevel}\n" +
                    "\n" +
                    "ALTER TABLE UnitTest_Person ADD FirstName NVARCHAR(255);\n" +
                    "ALTER TABLE UnitTest_Person ADD LastName NVARCHAR(255);\n" +
                    "\n" +
                    "DECLARE @sql NVARCHAR(2048) = 'UPDATE UnitTest_Person SET" +
                    "  FirstName = LEFT(Name, CHARINDEX('' '', Name)-1)" +
                    "  , LastName = LTRIM(RIGHT(Name, LEN(Name) - CHARINDEX('' '', Name)));';\n" +
                    "EXEC sys.sp_executesql @query = @sql;\n" +
                    "\n" +
                    "ALTER TABLE UnitTest_Person DROP COLUMN Name"
                );
                File.WriteAllText(_extraPatch2Rollback,
                    $"-- ROLLBACK: {newPatchLevel}\n" +
                    "\n" +
                    "ALTER TABLE UnitTest_Person ADD Name NVARCHAR(255);\n" +
                    "\n" +
                    "DECLARE @sql NVARCHAR(2048) = 'UPDATE UnitTest_Person SET Name = FirstName + '' '' + LastName;';\n" +
                    "EXEC sys.sp_executesql @query = @sql;\n" +
                    "\n" +
                    "ALTER TABLE UnitTest_Person DROP COLUMN FirstName;\n" +
                    "ALTER TABLE UnitTest_Person DROP COLUMN LastName;"
                );
            }

            private void DeleteExtraPatch1Files()
            {
                new FileInfo(_extraPatch1).Delete();
                new FileInfo(_extraPatch1Rollback).Delete();
            }

            private void DeleteExtraPatch2Files()
            {
                new FileInfo(_extraPatch2).Delete();
                new FileInfo(_extraPatch2Rollback).Delete();
            }
        }
    }
}
