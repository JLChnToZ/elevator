﻿using System;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Elevator {
    internal abstract class HandlerBase {
        private static readonly Regex validateChar = new Regex("[\x00\x0a\x0d]");
        private static readonly Regex checkQuotesRequired = new Regex("\\s|\\\"\\\"");
        private static readonly Regex escapeMatcher = new Regex("(\\\\*)(\\\"\\\"|$)");

        protected readonly ProcessStartInfo startInfo = new ProcessStartInfo();
        private readonly StringBuilder newArgs = new StringBuilder();
        protected WaitMode wait;
        protected int stopIndex;

        protected HandlerBase(ParsedInfo parsedInfo) {
            stopIndex = parsedInfo.stopIndex;

            InitStartInfo(parsedInfo);
            for(int i = 0; i < parsedInfo.stopIndex; i++)
                HandleArgument(i, parsedInfo.matches[i]);
            FinalizeStartInfo(parsedInfo.args);
        }

        public int Launch() {
            var process = Process.Start(startInfo);
            if(wait != WaitMode.Wait)
                return 0;
            process.WaitForExit();
            return process.ExitCode;
        }

        protected abstract void InitStartInfo(ParsedInfo info);

        protected abstract void HandleArgument(int index, Match match);

        protected virtual void FinalizeStartInfo(string[] args) {
            for(int i = stopIndex; i < args.Length; i++)
                AppendArgument(args[i]);
            startInfo.Arguments = newArgs.ToString();
        }

        protected void AppendArgument(Match m) => AppendArgument(m.Value);

        protected void AppendArgument(string arg) {
            if(arg == null) arg = string.Empty;
            if(validateChar.IsMatch(arg))
                throw new ArgumentOutOfRangeException(nameof(arg));
            if(newArgs.Length > 0)
                newArgs.Append(' ');
            if(arg == string.Empty) {
                newArgs.Append("\"\"");
                return;
            }
            if(!checkQuotesRequired.IsMatch(arg)) {
                newArgs.Append(arg);
                return;
            }
            newArgs
                .Append('"')
                .Append(escapeMatcher.Replace(arg, EscapeReplace))
                .Append('"');
        }

        private static string EscapeReplace(Match match) {
            var group1 = match.Groups[1].Value;
            return string.Join(
                string.Empty,
                group1,
                group1,
                match.Groups[2].Value == "\"" ? "\\\"" : string.Empty
            );
        }
    }
}