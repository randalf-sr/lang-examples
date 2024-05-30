import { parseArgs } from "https://deno.land/std@0.213.0/cli/parse_args.ts";
import { basename } from "https://deno.land/std@0.213.0/path/basename.ts";

const args = parseArgs(Deno.args);
if (args._.length === 0) {
    const path = basename(Deno.execPath());
    const app = path.toLowerCase().endsWith(".exe") ? path.slice(0, -4) : path;
    console.error(`Usage ${app} <filename>`);
    Deno.exit(1);
}

const filename = args._[0];
try {
    if (typeof filename !== "string") {
        throw new Error("Filename is invalid");
    }

    Deno.writeFileSync(filename, new Uint8Array(), {
        createNew: true
    });

} catch (e) {
    console.error(e.message);
    Deno.exit(1);
}