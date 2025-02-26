using System;
using System.IO;
using System.Text;
using Microsoft.Build.Framework;
using Net.Sf.Dbdeploy;
using Net.Sf.Dbdeploy.Database;
using Net.Sf.Dbdeploy.Exceptions;

namespace MSBuild.Dbdeploy.Task
{
	public class Dbdeploy : ITask
	{
		private string dbType;
		private string dbConnection;
		private DirectoryInfo dir;
		private FileInfo outputfile;
		private FileInfo undoOutputfile;
		private int lastChangeToApply = Int32.MaxValue;
		private string deltaSet = "Main";
		private bool useTransaction;

	    public Dbdeploy()
	    {
	        TableName = DatabaseSchemaVersionManager.DEFAULT_TABLE_NAME;
	    }

	    [Required]
		public string DbType
		{
			set { dbType = value; }
		}

		[Required]
		public string DbConnection
		{
			set { dbConnection = value; }
		}

		[Required]
		public string Dir
		{
			get { return dir.FullName; }
			set { dir = new DirectoryInfo(value); }
		}

		[Required]
		public string OutputFile
		{
			get { return outputfile.FullName; }
			set { outputfile = new FileInfo(value); }
		}

		public string UndoOutputFile
		{
			get { return undoOutputfile.FullName; }
			set { undoOutputfile = new FileInfo(value); }
		}

		public int LastChangeToApply
		{
			get { return lastChangeToApply; }
			set { lastChangeToApply = value; }
		}

		public string DeltaSet
		{
			get { return deltaSet; }
			set { deltaSet = value; }
		}

		public bool UseTransaction
		{
			get { return useTransaction; }
			set { useTransaction = value; }
        }

        public string TableName { get; set; }

        public IBuildEngine BuildEngine { get; set; }

        public ITaskHost HostObject { get; set; }

		public bool Execute()
		{
			var result = false;
			try
			{
				LogTaskProperties();

			    using (TextWriter outputPrintStream = new StreamWriter(outputfile.FullName))
				{
					TextWriter undoOutputPrintStream = null;
					if (undoOutputfile != null)
					{
						undoOutputPrintStream = new StreamWriter(undoOutputfile.FullName);
					}
					var factory = new DbmsFactory(dbType, dbConnection);
					var dbmsSyntax = factory.CreateDbmsSyntax();
					var databaseSchemaVersion = new DatabaseSchemaVersionManager(factory, deltaSet, null, TableName);

					var toPrintSteamDeployer = new ToPrintStreamDeployer(databaseSchemaVersion, dir, outputPrintStream, dbmsSyntax, useTransaction, undoOutputPrintStream);
					toPrintSteamDeployer.DoDeploy(lastChangeToApply);

					if (undoOutputPrintStream != null)
					{
						undoOutputPrintStream.Close();
					}
				}
				result = true;
			}
			catch (DbDeployException ex)
			{
				Console.Error.WriteLine(ex.Message);
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine("Failed to apply changes: " + ex.Message);
				Console.Error.WriteLine("Stack Trace:");
				Console.Error.Write(ex.StackTrace);
			}
			return result;
		}

	    private void LogTaskProperties()
	    {
	        if (BuildEngine == null)
                return;

	        var builder = new StringBuilder();
	        builder.Append("DbType=");
	        builder.Append(dbType);
	        builder.AppendLine();
	        builder.Append("DbConnection=");
	        builder.Append(dbConnection);
	        builder.AppendLine();
	        builder.Append("Dir=");
	        builder.Append(dir);
	        builder.AppendLine();
	        builder.Append("OutputFile=");
	        builder.Append(outputfile);
	        builder.AppendLine();
	        builder.Append("UndoOutputFile=");
	        builder.Append(undoOutputfile);
	        builder.AppendLine();
	        builder.Append("LastChangeToApply=");
	        builder.Append(lastChangeToApply);
	        builder.AppendLine();
	        builder.Append("DeltaSet=");
	        builder.Append(deltaSet);
	        builder.AppendLine();

            Console.Error.WriteLine(builder.ToString());
	    }
	}
}