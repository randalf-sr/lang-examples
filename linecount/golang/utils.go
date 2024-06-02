package main

import (
	"errors"
	"flag"
	"fmt"
	"os"
	"regexp"
	"runtime"
)

func parseArgs() Config {
	filePath := flag.String("f", "", "file path.")
	chunkSize := flag.Int("c", 4*1024*1024, "chunk size.")
	maxConcurrency := flag.Int("m", runtime.NumCPU(), "max concurrency.")

	flag.Parse()

	if *filePath == "" {
		flag.Usage()
		os.Exit(1)
	}

	if *chunkSize <= 0 {
		fmt.Println("Chunk size must be greater than 0.")
		os.Exit(1)
	}

	if *maxConcurrency <= 0 {
		fmt.Println("Max concurrency must be greater than 0.")
		os.Exit(1)
	}

	fileInfo, err := os.Stat(*filePath)
	if err != nil {
		if errors.Is(err, os.ErrNotExist) {
			fmt.Printf("File %s does not exist\n", *filePath)
		} else {
			fmt.Printf("Failed to get file info: %v\n", err)
		}
		os.Exit(1)
	}

	runtime.GOMAXPROCS(*maxConcurrency)

	return Config{
		info:           fileInfo,
		filePath:       *filePath,
		chunkSize:      *chunkSize,
		maxConcurrency: *maxConcurrency,
	}
}

func formatCommas(num int64) string {
	str := fmt.Sprintf("%d", num)
	re := regexp.MustCompile(`(\d+)(\d{3})`)
	for n := ""; n != str; {
		n = str
		str = re.ReplaceAllString(str, "$1,$2")
	}
	return str
}

func formatToKb(num int64) string {
	if num < 1024 {
		return fmt.Sprintf("%d B", num)
	}

	return fmt.Sprintf("%s KB", formatCommas(num/1024))
}
