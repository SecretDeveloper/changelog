using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace changelog
{
    class ChangeLog
    {
        public Dictionary<string, string> _SupportedFormats = new Dictionary<string, string>()
        {
            {"markdown", "mk"}
        };
        
        static void Main(string[] args)
        {
            var currentPath = Directory.GetCurrentDirectory();
#if DEBUG
            Debugger.Launch();
            currentPath = "c:\\Temp";
#endif
            

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
                    changelogPath = CreateChangelogFile(currentPath);
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
            // only supporting MK now
            return true;
        }

        private static void AppendEntryToChangeLog(string changelogPath, string entry)
        {
            var content = File.ReadAllText(changelogPath);
            var ne = "---------------------------\r\n" + entry;
            File.WriteAllText(changelogPath, content.Replace("---------------------------\r\n", ne));
        }

        private static string CreateChangeLogEntry(bool isMarkdown)
        {
            string entryTemplate = Strings.mk_entry_template;
            string entrySectionTemplate = Strings.mk_entry_section; 
            string entryCommentTemplate = Strings.mk_entry_comment;

            var version = PromptForString("What is the version number for this entry?");
            entryTemplate = entryTemplate.Replace("{version}", "["+version+"]");

            entryTemplate = entryTemplate.Replace("{date}", DateTime.Now.ToString("yyyy-MMM-dd"));

            var input = RepeatPromptForString(String.Format(Strings.prompt_entry_template, "[ADDED]"), entryCommentTemplate);
            var section = entrySectionTemplate.Replace("{entry_type}", "[ADDED]").Replace("{entry_comments}", input);
            section += "\r\n";
            input = "";
            input += RepeatPromptForString(String.Format(Strings.prompt_entry_template, "[CHANGED]"), entryCommentTemplate);
            section += entrySectionTemplate.Replace("{entry_type}", "[CHANGED]").Replace("{entry_comments}", input);
            section += "\r\n";

            return entryTemplate.Replace("{entry_section}", section);
        }

        private static string RepeatPromptForString(string prompt, string template)
        {
            Console.Write(prompt);
            var input = "";

            var tmp = Console.ReadLine();
            while (tmp.Trim().Length > 0)
            {
                input += string.Format(template, tmp);
                tmp = Console.ReadLine();
            }
            
            return input.Trim();
        }

        private static string PromptForString(string prompt, string defaultValue="")
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            return string.IsNullOrEmpty(input) ? defaultValue : input;
        }

        private static string CreateChangelogFile(string path)
        {
            var changelogPath = Path.Combine(path, "changelog");

            var input = PromptForInput(Strings.prompt_createfile, "Y");
            var template = Strings.mk_template;

            input = PromptForInput(Strings.prompt_semver_support);
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

        private static string PromptForKeyInput(string prompt, string defaultValue = null)
        {
            var t = prompt;
            if (defaultValue != null) t = t + "(" + defaultValue + ")";

            Console.Write(t);

            var input = ReadKeyFromConsole();
            if (string.IsNullOrEmpty(input)) input = defaultValue;
            return input;
        }

        private static string PromptForInput(string prompt, string defaultValue = null)
        {
            var t = prompt;
            if (defaultValue != null) t = t + "(" + defaultValue + ")";

            Console.Write(t);

            var input = ReadKeyFromConsole();
            if (string.IsNullOrEmpty(input)) input = defaultValue;
            return input;
        }

        private static string ReadKeyFromConsole()
        {
            var r = Console.ReadKey(true).KeyChar.ToString();
            Console.WriteLine();
            return r;
        }
    }
}
