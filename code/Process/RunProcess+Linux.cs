namespace RJCP.Threading.Process
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public partial class RunProcess
    {
        /// <summary>
        /// Algorithms for process handling specific to Linux.
        /// </summary>
        public static class Linux
        {
            /// <summary>
            /// Splits the command line into its consitituents.
            /// </summary>
            /// <param name="arguments">The arguments string that needs to be split.</param>
            /// <returns>The list of arguments</returns>
            /// <remarks>
            /// This method splits arguments in a single line into multiple arguments similar to Linux. There is no
            /// guarantee that it works the same as what is on Linux (due to the various shells and C-library
            /// implementations).
            /// <para>
            /// When a quote is found, everything that is quoted inside is added to the argument. If the quote is
            /// escaped with a backslash, then the quote is treated verbatim. For example:
            /// </para>
            /// <list type="bullet">
            /// <item><c>"foo"</c> is converted to <c>foo</c>.</item>
            /// <item><c>"foo"bar</c> is converted to <c>foobar</c>.</item>
            /// <item><c>x" "y</c> is converted to <c>x y</c>.</item>
            /// <item>
            /// <c>"one"" two"</c> is converted to <c>one two</c> because there is no space between the non-quoted area.
            /// </item>
            /// </list>
            /// <para>
            /// When a backslash is found, it's used for escaping. If the next character is a quote or a backslash, it
            /// is added to the argument verbatim. All other characters result in the backslash and the character in the
            /// output. For example
            /// </para>
            /// <list type="bullet">
            /// <item><c>\\</c> is converted to <c>\</c></item>
            /// <item><c>\"</c> is converted to <c>"</c></item>
            /// <item><c>\n</c> is converted to <c>\n</c></item>
            /// </list>
            /// </remarks>
            public static string[] SplitCommandLine(string arguments)
            {
                if (string.IsNullOrWhiteSpace(arguments))
                    return Array.Empty<string>();

                StringBuilder arg = new();
                List<string> args = new();
                bool escape = false;
                bool quote = false;
                foreach (char c in arguments) {
                    if (!escape) {
                        if (c == Quote) {
                            quote = !quote;
                            continue;
                        } else if (char.IsWhiteSpace(c)) {
                            if (!quote) {
                                if (arg.Length > 0) {
                                    args.Add(arg.ToString());
                                    arg.Clear();
                                }
                                continue;
                            }
                        } else if (c == Backslash) {
                            escape = true;
                            continue;
                        }
                        arg.Append(c);
                    } else {
                        if (c is Quote or Backslash) {
                            arg.Append(c);
                        } else {
                            arg.Append(Backslash);
                            arg.Append(c);
                        }
                        escape = false;
                    }
                }

                if (escape)
                    arg.Append(Backslash);
                if (arg.Length != 0) args.Add(arg.ToString());

                return args.ToArray();
            }

            /// <summary>
            /// Joins the command line elements together that can be passed to start a process.
            /// </summary>
            /// <param name="arguments">The arguments to join.</param>
            /// <returns>The resulting string with all arguments joined.</returns>
            public static string JoinCommandLine(params string[] arguments)
            {
                if (arguments is null || arguments.Length == 0) return string.Empty;

                StringBuilder cmdLine = new();
                foreach (string arg in arguments) {
                    if (arg is null) continue;
                    AppendArgument(cmdLine, arg);
                }
                return cmdLine.ToString();
            }

            private static void AppendArgument(StringBuilder stringBuilder, string argument)
            {
                // Copied from https://github.com/dotnet/runtime/commit/ef2a1878793e7e3fc3060396d3d2655ac53b1316
                // src/libraries/System.Private.CoreLib/src/System/PasteArguments.cs

                // Licensed to the .NET Foundation under one or more agreements.
                // The .NET Foundation licenses this file to you under the MIT license.

                if (stringBuilder.Length != 0) {
                    stringBuilder.Append(' ');
                }

                // Parsing rules for non-argv[0] arguments:
                // - Backslash is a normal character except followed by a quote.
                // - 2N backslashes followed by a quote ==> N literal backslashes followed by unescaped quote
                // - 2N+1 backslashes followed by a quote ==> N literal backslashes followed by a literal quote
                // - Parsing stops at first whitespace outside of quoted region.
                // - (post 2008 rule): A closing quote followed by another quote ==> literal quote, and parsing remains
                //   in quoting mode.
                if (argument.Length != 0 && ContainsNoWhitespaceOrQuotes(argument)) {
                    // Simple case - no quoting or changes needed.
                    stringBuilder.Append(argument);
                } else {
                    stringBuilder.Append(Quote);
                    int idx = 0;
                    while (idx < argument.Length) {
                        char c = argument[idx++];
                        if (c == Backslash) {
                            int numBackSlash = 1;
                            while (idx < argument.Length && argument[idx] == Backslash) {
                                idx++;
                                numBackSlash++;
                            }

                            if (idx == argument.Length) {
                                // We'll emit an end quote after this so must double the number of backslashes.
                                stringBuilder.Append(Backslash, numBackSlash * 2);
                            } else if (argument[idx] == Quote) {
                                // Backslashes will be followed by a quote. Must double the number of backslashes.
                                stringBuilder.Append(Backslash, numBackSlash * 2 + 1);
                                stringBuilder.Append(Quote);
                                idx++;
                            } else {
                                // Backslash will not be followed by a quote, so emit as normal characters.
                                stringBuilder.Append(Backslash, numBackSlash);
                            }

                            continue;
                        }

                        if (c == Quote) {
                            // Escape the quote so it appears as a literal. This also guarantees that we won't end up
                            // generating a closing quote followed by another quote (which parses differently pre-2008
                            // vs. post-2008.)
                            stringBuilder.Append(Backslash);
                            stringBuilder.Append(Quote);
                            continue;
                        }

                        stringBuilder.Append(c);
                    }

                    stringBuilder.Append(Quote);
                }
            }

            private static bool ContainsNoWhitespaceOrQuotes(string s)
            {
                for (int i = 0; i < s.Length; i++) {
                    char c = s[i];
                    if (char.IsWhiteSpace(c) || c == Quote) {
                        return false;
                    }
                }

                return true;
            }

            private const char Quote = '\"';
            private const char Backslash = '\\';
        }
    }
}
