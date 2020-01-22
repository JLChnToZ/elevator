# Elevator
This is a utility application that can launch anything with everything below defined at the same time:

- Privilege raised
- Run as different user
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
- `--eXXX`: Copy environment variable `XXX` in current scope to new environment.
- `--aXXX=YYY`: Appends `YYY` to environment variable `XXX`.
- `--pXXX=YYY`: Prepends `YYY` to environment variable `XXX`.
- `--cd=YYY`: Sets working directory to `YYY`.
- `--wXXX`: Sets window style for new process, it can be `normal`, `hidden`, `minimized`, `maximized`, default is `normal`.
- `--nowindow`: Sets new process lanuch without creating a new window to contain it.
- `--nowait`: Do not wait until the target application exits. This may cause input problem if it is launched within interactive console.
- `--login=XXX`: Run the process with different user as `XXX`, you may pass as UPN format, such as `user@DNS_domain_name`.
- `--loginpw=XXX`: Password. If you have `login` defined and the user needs password, please provide with password.
- `--loaduserprofile`: Load user profile, only have use when use with `login`.

The option prefix can be `--`, `-`, or `/`.

## License
[MIT](LICENSE)
