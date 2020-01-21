# Elevator
This is a utility application that can launch anything with everything below defined at the same time:

- Privilege raised
- Environment variable maniupate (prefix, postfix, and/or redefine)
- Passing arguments
- Shell execute (Supports open something which itself isn't an executable)
- Working directory change

This utility is specified and targeted to Windows platform only, it will not work on other platform even you try to run it with other CLR implementations.

## Usage
`Elevator.exe [options] [--] {command} [arguments...]`

where options can be:
- `--vXXX`: Verb, aka. the action will take on the command/filename,
  define this to use shell execute, where `XXX` is the verb itself, it can be `open`, `edit`, `openasreadonly`, `print`, etc.
- `--runas`: Special action that raise privilege to administrator while launching.
- `--eXXX=YYY`: Sets `YYY` to environment variable `XXX`.
- `--aXXX=YYY`: Appends `YYY` to environment variable `XXX`.
- `--pXXX=YYY`: Prepends `YYY` to environment variable `XXX`.
- `--cd=YYY`: Sets working directory to `YYY`.
- `--wXXX`: Sets window style for new process, it can be `normal`, `hidden`, `minimized`, `maximized`, default is `normal`.
- `--nowindow`: Sets new process lanuch without creating a new window to contain it.
- `--userprofile`: Load user profile.
- `--nouserprofile`: Do not load user profile, this is the default and no need to explicit defined.
- `--cusername=XXX`: Run the process with different user as `XXX`, you may pass as UPN format, such as `user@DNS_domain_name`.
- `--cpassword=XXX`: Password. If you have `cusername` defined and the user needs password, please provide with password.

The option prefix can be `--`, `-`, or `/`.

## License
[MIT](LICENSE)
