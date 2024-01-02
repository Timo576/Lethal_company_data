"""Reads a file of voice commands and creates a maze then helps solve it."""


import time
from watchdog.observers import Observer
from watchdog.events import FileSystemEventHandler


# noinspection PyMethodParameters
class Handler(FileSystemEventHandler):
    """Handles the file system events."""

    def on_modified(event, **kwargs):
        """Event handler for when the file is modified.
        """
        print("Got it!")
        print(read_new_moves())


def read_new_moves():
    """Reads the file."""
    with open('current_map.txt') as file:
        moves = file.read()
    return moves


def main():
    """Main function."""
    path = "current_map.txt"
    event_handler = Handler()
    observer = Observer()
    observer.schedule(event_handler, path)
    observer.start()
    try:
        while True:
            time.sleep(1)
    except KeyboardInterrupt:
        observer.stop()
    observer.join()


if __name__ == "__main__":
    main()
