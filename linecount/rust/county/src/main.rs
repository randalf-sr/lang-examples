use num_cpus;
use std::fs::File;
use std::io::{self, Read, Seek, SeekFrom};
use std::sync::{Arc, Mutex};
use std::thread;
use std::time::Instant;

struct Config {
    file_path: String,
    chunk_size: u64,
    max_concurrency: usize,
}

fn count_lines(cfg: Config, start: u64, end: u64, line_count_chan: Arc<Mutex<Vec<u64>>>) {
    let mut file = File::open(&cfg.file_path).expect("Failed to open file");
    file.seek(SeekFrom::Start(start)).unwrap();

    let mut buf_size = cfg.chunk_size;
    if end - start < buf_size {
        buf_size = end - start;
    }

    let mut buf = vec![0; buf_size as usize];
    let mut line_count = 0;

    for _ in (start..end).step_by(buf_size as usize) {
        match file.read_exact(&mut buf) {
            Ok(_) => {
                line_count += buf.iter().filter(|&&x| x == b'\n').count() as u64;
            }
            Err(e) => {
                if e.kind() != io::ErrorKind::UnexpectedEof {
                    panic!("Failed to read file: {:?}", e);
                }
                break;
            }
        }
    }

    line_count_chan.lock().unwrap().push(line_count);
}

fn do_count(cfg: &Config) -> u64 {
    let partitions = cfg.max_concurrency;
    let file_size = std::fs::metadata(&cfg.file_path).unwrap().len();
    let partition_size = (file_size + partitions as u64 - 1) / partitions as u64;
    let line_count_chan = Arc::new(Mutex::new(Vec::new()));

    let mut handles = vec![];

    for i in 0..partitions {
        let start = i as u64 * partition_size;
        let line_count_chan = Arc::clone(&line_count_chan);
        let cfg = Config {
            file_path: cfg.file_path.clone(),
            chunk_size: cfg.chunk_size,
            max_concurrency: cfg.max_concurrency,
        };
        handles.push(thread::spawn(move || {
            count_lines(cfg, start, start + partition_size, line_count_chan);
        }));
    }

    for handle in handles {
        handle.join().unwrap();
    }

    let count = line_count_chan.lock().unwrap().iter().sum();

    return count;
}

fn main() {
    let mut max_concurrency = num_cpus::get();
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

    if args.len() > 2 {
        max_concurrency = args[2].parse().unwrap();
    }

    let start = Instant::now();
    let cfg = Config {
        file_path: args[1].clone(),
        chunk_size: 1024 * 1024 * 8,
        max_concurrency,
    };
    let total_lines = do_count(&cfg);
    println!("  Total lines: {}", total_lines);
    println!("Total threads: {}", max_concurrency);
    println!(" Time elapsed: {:?}", start.elapsed());
}
