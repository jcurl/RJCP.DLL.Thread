namespace RJCP.Threading.Process
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class RunProcessCmdLineLinuxTest
    {
        private static readonly object[] SplitArguments = {
            new object[] { null, Array.Empty<string>()},
            new object[] { "", Array.Empty<string>()},
            new object[] { "/c dir", new string[] { "/c", "dir" }},
            new object[] { "\"She said \"you can't do this!\", didn't she?\"", new string[] { "She said you", "can't", "do", "this!, didn't she?" }},
            new object[] { "test.exe \"c:\\Path With Spaces\\Ending In Backslash\\\" Arg2 Arg3", new string[] { "test.exe", "c:\\Path With Spaces\\Ending In Backslash\" Arg2 Arg3" }},
            new object[] { "test.exe \"c:\\Path With Spaces\\Ending In Backslash\\\\\" Arg2 Arg3", new string[] { "test.exe", "c:\\Path With Spaces\\Ending In Backslash\\", "Arg2", "Arg3" }},
            new object[] { "DumpArgs foo\"\"\"\"\"\"\"\"\"\"\"\"bar", new string[] { "DumpArgs", "foobar" }},
            new object[] { "FinalProgram.exe \"first second \"\"embedded quote\"\" third\"", new string[] { "FinalProgram.exe", "first second embedded quote third" }},
            new object[] { "\"F\"i\"r\"s\"t S\"e\"c\"o\"n\"d\" T\"h\"i\"r\"d\"", new string[] { "First Second Third" }},
            new object[] { "F\"\"ir\"s\"\"t \\\"Second Third\"", new string[] { "First \"Second Third" } },
            new object[] { "Firs\\\"t \\\"Second Third", new string[] { "Firs\"t", "\"Second", "Third" }},
            new object[] { "\"First \\\"Second Third", new string[] { "First \"Second Third" }},
            new object[] { "  Something Else", new string[] { "Something", "Else" }},
            new object[] { "\"  \"Something Else", new string[] { "  Something", "Else" }},
            new object[] { "123 456\tabc\\def\"ghi\"", new string[] { "123", "456", "abc\\defghi" }},
            new object[] { "123\"456\"\tabc", new string[] { "123456", "abc" }},
        };

        // Anything with a space or quote is wrapped around new quotes. Backslashes and quotes are escaped.
        private static readonly object[] JoinedArguments = {
            new object[] { "", Array.Empty<string>()},
            new object[] { "/c dir", new string[] { "/c", "dir" }},
            new object[] { "\"She said you\" can't do \"this!, didn't she?\"", new string[] { "She said you", "can't", "do", "this!, didn't she?" }},
            new object[] { "\"c:\\Path With Spaces\\Ending In Backslash\\\" Arg2 Arg3\"", new string[] { "c:\\Path With Spaces\\Ending In Backslash\" Arg2 Arg3" }},
            new object[] { "\"c:\\Path With Spaces\\Ending In Backslash\\\\\" Arg2 Arg3", new string[] { "c:\\Path With Spaces\\Ending In Backslash\\", "Arg2", "Arg3" }},
            new object[] { "DumpArgs \"foo\\\"\\\"\\\"\\\"bar\"", new string[] { "DumpArgs", "foo\"\"\"\"bar" }},
            new object[] { "\"first second \\\"embedded\" quote third", new string[] { "first second \"embedded", "quote", "third" }},
            new object[] { "\"First Second Third\"", new string[] { "First Second Third" }},
            new object[] { "\"Firs\\\"t\" \"\\\"Second\" Third", new string[] { "Firs\"t", "\"Second", "Third" }},
            new object[] { "\"First \\\"Second Third\"", new string[] { "First \"Second Third" }},
            new object[] { "Something Else", new string[] { "Something", "Else" }},
            new object[] { "123 456 abc\\defghi", new string[] { "123", "456", "abc\\defghi" }},
            new object[] { "123456 abc", new string[] { "123456", "abc" }},
        };

        // Splitting (right to left) results in a different result than joining (left to right).
        private static readonly object[] SplitArguments2 = {
            new object[] { "\\\\SomeComputer\\subdir1\\subdir2\\", new string[] { "\\SomeComputer\\subdir1\\subdir2\\" }},
            new object[] { "\\\\SomeComputer\\subdir1\\subdir2\\\\", new string[] { "\\SomeComputer\\subdir1\\subdir2\\" }}
        };

        // Splitting (right to left) results in a different result than joining (left to right).
        private static readonly object[] JoinedArguments2 = {
            new object[] { "\\\\SomeComputer\\subdir1\\subdir2\\", new string[] { "\\\\SomeComputer\\subdir1\\subdir2\\" }},
            new object[] { "\\\\SomeComputer\\subdir1\\subdir2\\\\", new string[] { "\\\\SomeComputer\\subdir1\\subdir2\\\\" }}
        };

        [TestCaseSource(nameof(SplitArguments))]
        [TestCaseSource(nameof(SplitArguments2))]
        public void SplitArgument(string arguments, string[] expected)
        {
            string[] args = RunProcess.Linux.SplitCommandLine(arguments);
            Assert.That(args, Is.EqualTo(expected).AsCollection);
        }

        [TestCaseSource(nameof(JoinedArguments))]
        [TestCaseSource(nameof(JoinedArguments2))]
        public void JoinArgument(string joined, string[] arguments)
        {
            string cmdLine = RunProcess.Linux.JoinCommandLine(arguments);
            Assert.That(cmdLine, Is.EqualTo(joined));
        }

        [Test]
        public void JoinNullArguments()
        {
            string cmdLine = RunProcess.Linux.JoinCommandLine(null);
            Assert.That(cmdLine, Is.EqualTo(string.Empty));
        }

        [Test]
        public void JoinNullArgument()
        {
            string cmdLine = RunProcess.Linux.JoinCommandLine(new string[] { null });
            Assert.That(cmdLine, Is.EqualTo(string.Empty));
        }

        [Test]
        public void JoinNullArgumentStart()
        {
            string cmdLine = RunProcess.Linux.JoinCommandLine(new[] { null, "a" });
            Assert.That(cmdLine, Is.EqualTo("a"));
        }

        [Test]
        public void JoinNullArgumentEnd()
        {
            string cmdLine = RunProcess.Linux.JoinCommandLine(new[] { "b", null });
            Assert.That(cmdLine, Is.EqualTo("b"));
        }

        [Test]
        public void JoinNullArgumentMiddle()
        {
            string cmdLine = RunProcess.Linux.JoinCommandLine(new[] { "x", null, "y" });
            Assert.That(cmdLine, Is.EqualTo("x y"));
        }
    }
}
