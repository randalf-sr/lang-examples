fn main() {
    let args: Vec<String> = std::env::args().collect();
    if args.len() < 2 {
        let exe_name = std::path::Path::new(&args[0])
            .file_stem()
            .unwrap()
            .to_str()
            .unwrap();

        eprintln!("Usage: {} <filename>", exe_name);
        std::process::exit(1);
    }

    let filename = &args[1];
    match std::fs::File::create_new(filename) {
        Ok(_) => (),
        Err(e) => eprintln!("{}", e),
    }
}
