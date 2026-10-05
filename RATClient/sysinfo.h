#pragma once

// buf receives a UTF-8 string.  cap is the buffer size in bytes.
void GetOsString      (char* buf, int cap);   // e.g. "Windows 11 Pro (Build 22631)"
void GetHostnameString(char* buf, int cap);   // computer name
