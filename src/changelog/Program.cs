using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace changelog
{
    class Program
    {
        public Dictionary<string, string> _SupportedFormats = new Dictionary<string, string>()
        {
            {"markdown", "mk"},
            {"text", "txt"}
        };
        
        static void Main(string[] args)
        {

#if DEBUG
            Debugger.Launch();
#endif


            var currentPath = Directory.GetCurrentDirectory();
            // Find current changelog file
            string changelogPath = Directory.GetFiles(currentPath)
                .FirstOrDefault(
                    x =>
                    {
                        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(x);
                        return fileNameWithoutExtension != null && fileNameWithoutExtension
                                 .Equals("changelog", StringComparison.CurrentCultureIgnoreCase);
                    });

            
            if (string.IsNullOrEmpty(changelogPath))
            {
                // prompt to create file y/n
                Console.Write(Strings.prompt_createfile);
                var input = ReadKeyFromConsole();
                // create file or exit    
                if (input.Equals("Y", StringComparison.CurrentCultureIgnoreCase))
                {
                    changelogPath = CreateNewChanglog(currentPath);
                }
                else
                    return;
            }
            
            // create entry type
            var entry = CreateChangeLogEntry(isMarkdown(changelogPath));

            AppendEntryToChangeLog(changelogPath, entry);
        }

        private static bool isMarkdown(string changelogPath)
        {
            // about as naive as you can get right now.
            var extension = Path.GetExtension(changelogPath)??"";
            if (extension.Equals(".txt", StringComparison.CurrentCultureIgnoreCase)) return false;

            return true;
        }

        private static void AppendEntryToChangeLog(string changelogPath, string entry)
        {
            Console.WriteLine(entry);
        }

        private static string CreateChangeLogEntry(bool isMarkdown)
        {
            string ret = Strings.mk_entry_template;
            string entryTemplate = Strings.mk_entry_template;
            string commentTemplate = Strings.mk_entry_comment;
            if (!isMarkdown)
            {
                ret = Strings.txt_entry_template;
                entryTemplate = Strings.txt_entry_template;
                commentTemplate = Strings.txt_entry_comment;
            }

            var version = PromptForString("What is the version number for this entry?");
            entryTemplate = entryTemplate.Replace("{version}", "["+version+"]");

            entryTemplate = entryTemplate.Replace("{date}", DateTime.Now.ToString("yyyy-MMM-dd"));

            var input = RepeatPromptForString(String.Format(Strings.prompt_entry_template, "[ADDED]"), commentTemplate.Replace("{entry_type}", "[ADDED]"));
            input += RepeatPromptForString(String.Format(Strings.prompt_entry_template, "[CHANGED]"), commentTemplate.Replace("{entry_type}", "[CHANGED]"));

            return entryTemplate.Replace("{entry_comments}", input);
        }

        private static string RepeatPromptForString(string prompt, string template)
        {
            Console.Write(prompt);
            var input = "";

            var tmp = Console.ReadLine();
            while (tmp.Length > 0)
            {
                tmp = Console.ReadLine();
                input += Environment.NewLine + string.Format(template, tmp);
            }
            
            return input;
        }

        private static string PromptForString(string prompt, string defaultValue="")
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            return string.IsNullOrEmpty(input) ? defaultValue : input;
        }

        private static string CreateNewChanglog(string path)
        {

            var changelogPath = Path.Combine(path, "changelog");

            Console.Write(Strings.prompt_filetype);
            var input = ReadKeyFromConsole();
            var template = Strings.mk_template;
            if (input.Equals("T", StringComparison.CurrentCultureIgnoreCase))
            {
                template = Strings.txt_template;
                changelogPath += ".txt";
            }
            else
            {
                changelogPath += ".md";
            }

            Console.Write(Strings.prompt_semver_support);
            input = ReadKeyFromConsole();
            if (input.Equals("N", StringComparison.CurrentCultureIgnoreCase))
                template = template.Replace("{semver_comment}", ""); // remove Semver line.

            var found = Regex.Matches(template, @"\{.*\}");
            var captured = found
                .Cast<Match>()
                .SelectMany(o => o.Groups.Cast<Capture>().Select(c => c.Value));

            foreach(var match in captured)
            {
                template = template.Replace(match, Strings.ResourceManager.GetString(match.Trim(new []{'{','}'})));
            }
            // write to disk.
            File.WriteAllText(changelogPath, template);
            return changelogPath; 
        }

        private static string ReadKeyFromConsole()
        {
            var r = Console.ReadKey(true).KeyChar.ToString();
            Console.WriteLine();
            return r;
        }
    }
}
