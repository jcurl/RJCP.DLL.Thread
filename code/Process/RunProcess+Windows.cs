namespace RJCP.Threading.Process
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public partial class RunProcess
    {
        /// <summary>
        /// Algorithms for process handling specific to Windows
        /// </summary>
        public static class Windows
        {
            /// <summary>
            /// Splits the command line into its consitituents, the same was that MSVC does it.
            /// </summary>
            /// <param name="arguments">The arguments string that needs to be split.</param>
            /// <returns>The list of arguments</returns>
            /// <remarks>
            /// This method works for the arguments to the <see cref="System.Diagnostics.Process"/> class when creating
            /// when the command is not empty or null. That is, it splits correction assuming this is the second
            /// argument and following (assuming the first argument is the process that you wish to execute).
            /// <para>It has been tested to match the API call <c>CommandLineToArgvW</c> for its parsing.</para>
            /// <para>
            /// For more information, refer to the web site
            /// http://www.windowsinspired.com/how-a-windows-programs-splits-its-command-line-into-individual-arguments/
            /// </para>
            /// </remarks>
            public static string[] SplitCommandLine(string arguments)
            {
                if (string.IsNullOrWhiteSpace(arguments))
                    return Array.Empty<string>();

                StringBuilder arg = new();
                List<string> args = new();
                bool special = false;
                bool escape = false;
                bool quote = false;
                int backslashcount = 0;
                foreach (char c in arguments) {
                    if (!escape) {
                        if (c == '\"') {
                            if (!quote) {
                                quote = true;
                            } else {
                                if (special) {
                                    quote = false;
                                    arg.Append('\"');
                                }
                                special = !special;
                            }
                            continue;
                        }
                        if (quote) {
                            quote = false;
                            special = !special;
                        }

                        if (c is ' ' or '\t') {
                            if (!special) {
                                if (arg.Length != 0) {
                                    args.Add(arg.ToString());
                                    arg.Clear();
                                }
                            } else {
                                arg.Append(' ');
                            }
                            continue;
                        }
                        if (c == '\\') {
                            escape = true;
                            backslashcount = 1;
                            continue;
                        }
                        arg.Append(c);
                    } else {
                        if (c == '\\') {
                            backslashcount++;
                            continue;
                        }
                        if (c == '\"') {
                            arg.Append('\\', backslashcount / 2);
                            if (backslashcount % 2 == 0) {
                                special = !special;
                            } else {
                                arg.Append('\"');
                            }
                            escape = false;
                            continue;
                        }
                        arg.Append('\\', backslashcount);
                        if (c is >= (char)1 and <= (char)32) {
                            if (!special) {
                                if (arg.Length != 0) {
                                    args.Add(arg.ToString());
                                    arg.Clear();
                                }
                            } else {
                                arg.Append(' ');
                            }
                        } else {
                            arg.Append(c);
                        }
                        escape = false;
                    }
                }

                if (escape)
                    arg.Append('\\', backslashcount);
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
                StringBuilder cmdLine = new();
                StringBuilder escArg = new();
                foreach (string arg in arguments) {
                    bool quote = false;
                    foreach (char c in arg) {
                        if (c is ' ' or '\t') {
                            quote = true;
                        } else if (c == '\"') {
                            escArg.Append('\\');
                        }
                        escArg.Append(c);
                    }
                    if (cmdLine.Length > 0) cmdLine.Append(' ');
                    if (quote) {
#if NETCOREAPP
                        if (arg[^1] == '\\')
                            escArg.Append('\\');
#else
                        if (arg[arg.Length - 1] == '\\')
                            escArg.Append('\\');
#endif
                        cmdLine.Append('"').Append(escArg).Append('"');
                    } else {
                        cmdLine.Append(escArg);
                    }
                    escArg.Clear();
                }
                return cmdLine.ToString();
            }
        }
    }
}
