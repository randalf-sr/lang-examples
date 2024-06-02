package main

import (
	"bytes"
	"fmt"
	"io"
	"io/fs"
	"os"
	"runtime"
	"sync"
	"time"
)

func countLines(cfg Config, start int64, end int64, wg *sync.WaitGroup, lineCountChan chan<- int64) {
	defer wg.Done()

	file, err := os.Open(cfg.filePath)
	if err != nil {
		fmt.Printf("Failed to open file: %v", err)
		return
	}
	defer file.Close()

	bufSize := cfg.chunkSize
	if end-start < int64(bufSize) {
		bufSize = int(end - start)
	}

	buf := make([]byte, bufSize)
	lineSep := []byte{'\n'}
	lineCount := int64(0)

	for off := start; off < end; off += int64(bufSize) {
		n, err := file.ReadAt(buf, off)
		lineCount += int64(bytes.Count(buf[:n], lineSep))

		if err != nil {
			if err != io.EOF {
				fmt.Printf("Failed to read file: %v", err)
			}
			break
		}
	}

	lineCountChan <- lineCount
}

func doCount(cfg Config) int64 {
	partitions := runtime.NumCPU()
	partitionSize := (cfg.info.Size() + int64(partitions) - 1) / int64(partitions)
	lineCountChan := make(chan int64, partitions)

	wg := sync.WaitGroup{}
	for i := 0; i < partitions; i++ {
		wg.Add(1)
		start := int64(i) * partitionSize
		go countLines(cfg, start, start+partitionSize, &wg, lineCountChan)
	}

	go func() {
		wg.Wait()
		close(lineCountChan)
	}()

	totalLines := int64(0)
	for count := range lineCountChan {
		totalLines += count
	}

	return totalLines
}

func main() {
	start := time.Now()
	cfg := parseArgs()
	printResults(cfg, doCount(cfg), start)
}

func printResults(cfg Config, linesCounted int64, start time.Time) {
	fmt.Printf("   File name: %s\n", cfg.info.Name())
	fmt.Printf("   File size: %s\n", formatToKb(cfg.info.Size()))
	fmt.Printf("  Line count: %s\n", formatCommas(linesCounted))
	fmt.Printf("  Cores used: %d\n", runtime.NumCPU())
	fmt.Printf("Time elapsed: %.dm\n", time.Since(start).Milliseconds())
}

type Config struct {
	info      fs.FileInfo
	filePath  string
	chunkSize int
}
