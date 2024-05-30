package main

import (
	"errors"
	"fmt"
	"log"
	"os"
	"strings"
)

func main() {
	p, err := os.Executable()
	if err != nil {
		log.Fatal(err)
	}

	st, err := os.Stat(p)
	if err != nil {
		log.Fatal(err)
	}

	if len(os.Args) < 2 {
		exe := st.Name()
		if strings.HasSuffix(strings.ToLower(exe), ".exe") {
			exe = exe[:len(st.Name())-4]
		}
		fmt.Printf("Usage: %s <filename>\n", exe)
		os.Exit(1)
	}

	if checkFileExists(os.Args[1]) {
		fmt.Println("File already exists")
		os.Exit(1)
	}

	f, err := os.OpenFile(os.Args[1], os.O_CREATE|os.O_WRONLY, 0644)
	if err != nil {
		log.Fatal(err)
	}

	defer f.Close()
}

func checkFileExists(filePath string) bool {
	_, error := os.Stat(filePath)
	return !errors.Is(error, os.ErrNotExist)
}
