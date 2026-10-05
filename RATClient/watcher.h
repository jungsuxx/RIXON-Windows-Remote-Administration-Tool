#pragma once

// Writes the UTF-8 title of the current foreground window into buf[cap].
// Writes an empty string if no foreground window or title is blank.
void GetActiveWindowTitle(char* buf, int cap);
