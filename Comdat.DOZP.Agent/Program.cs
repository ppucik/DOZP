using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Comdat.DOZP.Core;
using Comdat.DOZP.Data;
using Comdat.DOZP.Data.Business;
using Comdat.DOZP.Data.Repository;

namespace Comdat.DOZP.Agent
{
    class Program
    {
        #region Constants
        const string APP_NAME = "Comdat.DOZP.Agent";
        #endregion

        static void Main(string[] args)
        {
            try
            {
                AssemblyName assembly = Assembly.GetExecutingAssembly().GetName();
                Version version = assembly.Version;
                Console.WriteLine(String.Format("{0} [Verze {1}.{2}.{3}], (c) Copyright 2014-2015 Comdat s.r.o.\n", assembly.Name, version.Major, version.Minor, version.Revision));

                if ((args == null) || (args.Length != 1))
                {
                    ShowHelp();
                    return;
                }

                foreach (string arg in args)
                {
                    if (arg.StartsWith("-"))
                    {
                        switch (arg.ToUpper())
                        {
                            case "-I":
                                throw new NotImplementedException();
                            case "-E":
                                Export();
                                break;
                            case "-R":
                                Rename();
                                break;
                            case "-?":
                                ShowHelp();
                                return;
                        }
                    }
                    else
                    {
                        ShowHelp();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("Error: {0}", ex.Message));
            }
            finally
            {
                Console.WriteLine("Finished");
#if DEBUG
                Console.ReadLine();
#endif
            }
        }

        private static void Export()
        {
            try
            {
                List<Catalogue> catalogues = CatalogueComponent.Instance.GetList(new CatalogueFilter(true));

                if (catalogues != null)
                {
                    foreach (var catalogue in catalogues)
                    {
                        int result = CatalogueComponent.Instance.Export(catalogue.CatalogueID, UserConstants.SYSTEM, Environment.MachineName);
                        Console.WriteLine(String.Format("Katalog '{0}' exportováno {1} záznamů", catalogue.Name, result));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Util.WriteEventLog(String.Format("Chyba při exportu z TOCu: {0}", ex.Message), EventLogEntryType.Error);
            }
        }

        private static void Rename()
        {
            try
            {
                ScanFileRepository repository = new ScanFileRepository();
                List<Book> books = BookComponent.Instance.GetList(new BookFilter());
                int n = 0;

                if (books != null)
                {
                    foreach (var book in books)
                    {
                        string bookPath = book.GetDirectoryPath();
                        string fileName = book.GetFileName();
                        n++;

                        foreach (var file in book.ScanFiles)
                        {
                            string extension = Path.GetExtension(file.FileName);
                            string newFileName = String.Format("{0}{1}", fileName, extension);

                            if (file.FileName != newFileName)
                            {
                                string sourceFilePath = file.GetScanFilePath();
                                string destFilePath = Path.Combine(bookPath, newFileName);
                                File.Move(sourceFilePath, destFilePath);                            
                                Console.WriteLine(String.Format("[{0}] Soubor '{1}' -> {2}", n, file.FileName, destFilePath));

                                if (file.PartOfBook == PartOfBook.TableOfContents &&
                                    (file.Status == StatusCode.Complete || file.Status == StatusCode.Exported))
                                {
                                    sourceFilePath = file.GetOcrFilePath();
                                    extension = Path.GetExtension(sourceFilePath);
                                    destFilePath = Path.Combine(bookPath, String.Format("{0}{1}", fileName, extension));
                                    File.Move(sourceFilePath, destFilePath);
                                    Console.WriteLine(String.Format("[{0}] Soubor '{1}' -> {2}", n, file.OcrFileName, destFilePath));
                                }

                                file.FileName = newFileName;
                                repository.Update(file);
                            }
                            else
                            {
                                Console.WriteLine(String.Format("[{0}] Soubor '{1}' bez změny", n, file.FileName));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine("Použití: Comdat.DOZP.Agent [-I] nebo [-E]");
            Console.WriteLine("  -I  import nových záznamů z ALEPHu");
            Console.WriteLine("  -E  export souborů zpracovaných záznamů");
            Console.WriteLine("  -?  zobrazí přehled syntaxe parametrů");
            Console.ReadLine();
        }
    }
}
