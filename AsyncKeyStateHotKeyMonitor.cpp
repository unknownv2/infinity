#include "AsyncKeyStateHotKeyMonitor.h"
#include <Xinput.h>
#pragma comment(lib, "Xinput9_1_0.lib")

AsyncKeyStateHotKeyMonitor::AsyncKeyStateHotKeyMonitor(DWORD dwHotKeys[512])
{
	hMonitorEvent_ = nullptr;
	handler_ = nullptr;

	memcpy_s(hotKeys_, sizeof(hotKeys_), dwHotKeys, sizeof(DWORD) * 512);
}

AsyncKeyStateHotKeyMonitor::~AsyncKeyStateHotKeyMonitor()
{
	if (hMonitorEvent_ != nullptr)
	{
		SetEvent(hMonitorEvent_);
	}
}

bool AsyncKeyStateHotKeyMonitor::Start()
{
	if (hMonitorEvent_ != nullptr && WaitForSingleObject(hMonitorEvent_, 0) == WAIT_OBJECT_0)
	{
		return true;
	}

	hMonitorEvent_ = CreateEvent(nullptr, TRUE, FALSE, L"");

	if (hMonitorEvent_ == nullptr)
	{
		return false;
	}

	DWORD dwThreadId;

	auto hMonitorThread = CreateThread(nullptr, 0, MonitorProc, static_cast<void*>(this), 0, &dwThreadId);

	if (hMonitorThread == nullptr)
	{
		CloseHandle(hMonitorEvent_);
		return false;
	}

	// This does not terminate the thread.
	CloseHandle(hMonitorThread);

	return true;
}

void AsyncKeyStateHotKeyMonitor::Terminate()
{
	if (hMonitorEvent_ != nullptr)
	{
		SetEvent(hMonitorEvent_);
		hMonitorEvent_ = nullptr;
	}
}

void AsyncKeyStateHotKeyMonitor::SetOnHotKeyPressedHandler(HotKeyPressedHandlerT handler)
{
	handler_ = handler;
}

void AsyncKeyStateHotKeyMonitor::FillXInputs(BYTE keyStates[256], DWORD &dwLastPacket)
{
	XINPUT_STATE xState;

	if (XInputGetState(0, &xState) != ERROR_SUCCESS)
	{
		dwLastPacket = 0;
		
		for (auto x = VK_XINPUT_MIN_VALUE; x <= VK_XINPUT_MAX_VALUE; x++)
		{
			keyStates[x] = 0;
		}

		return;
	}

	if (xState.dwPacketNumber == dwLastPacket)
	{
		// The keys are already valid.
		return;
	}

	dwLastPacket = xState.dwPacketNumber;
	auto wButtons = xState.Gamepad.wButtons;

	keyStates[VK_XINPUT_LT] = xState.Gamepad.bLeftTrigger >= 180 ? KEY_DOWN : 0;
	keyStates[VK_XINPUT_RT] = xState.Gamepad.bRightTrigger >= 180 ? KEY_DOWN : 0;

	keyStates[VK_XINPUT_DPAD_UP] = (wButtons & XINPUT_GAMEPAD_DPAD_UP) == 0 ? 0 : KEY_DOWN;
	keyStates[VK_XINPUT_DPAD_DOWN] = (wButtons & XINPUT_GAMEPAD_DPAD_DOWN) == 0 ? 0 : KEY_DOWN;
	keyStates[VK_XINPUT_DPAD_LEFT] = (wButtons & XINPUT_GAMEPAD_DPAD_LEFT) == 0 ? 0 : KEY_DOWN;
	keyStates[VK_XINPUT_DPAD_RIGHT] = (wButtons & XINPUT_GAMEPAD_DPAD_RIGHT) == 0 ? 0 : KEY_DOWN;
	keyStates[VK_XINPUT_START] = (wButtons & XINPUT_GAMEPAD_START) == 0 ? 0 : KEY_DOWN;
	keyStates[VK_XINPUT_BACK] = (wButtons & XINPUT_GAMEPAD_BACK) == 0 ? 0 : KEY_DOWN;
	keyStates[VK_XINPUT_LSTICK] = (wButtons & XINPUT_GAMEPAD_LEFT_THUMB) == 0 ? 0 : KEY_DOWN;
	keyStates[VK_XINPUT_RSTICK] = (wButtons & XINPUT_GAMEPAD_RIGHT_THUMB) == 0 ? 0 : KEY_DOWN;
	keyStates[VK_XINPUT_LB] = (wButtons & XINPUT_GAMEPAD_LEFT_SHOULDER) == 0 ? 0 : KEY_DOWN;
	keyStates[VK_XINPUT_RB] = (wButtons & XINPUT_GAMEPAD_RIGHT_SHOULDER) == 0 ? 0 : KEY_DOWN;
	keyStates[VK_XINPUT_A] = (wButtons & XINPUT_GAMEPAD_A) == 0 ? 0 : KEY_DOWN;
	keyStates[VK_XINPUT_B] = (wButtons & XINPUT_GAMEPAD_B) == 0 ? 0 : KEY_DOWN;
	keyStates[VK_XINPUT_X] = (wButtons & XINPUT_GAMEPAD_X) == 0 ? 0 : KEY_DOWN;
	keyStates[VK_XINPUT_Y] = (wButtons & XINPUT_GAMEPAD_Y) == 0 ? 0 : KEY_DOWN;
}

DWORD AsyncKeyStateHotKeyMonitor::MonitorProc(LPVOID lpParameter)
{
	auto monitor = static_cast<AsyncKeyStateHotKeyMonitor*>(lpParameter);
	auto hEvent = monitor->hMonitorEvent_;
	auto bMonitorXInput = false;
	DWORD dwLastXInputPacket = 0;

	DWORD hotKeys[512];
	BYTE keyStates[256];
	BYTE hotKeyDownCounters[512];
	
	// Copy locally for speed.
	memcpy_s(hotKeys, sizeof(hotKeys), monitor->hotKeys_, sizeof(hotKeys_));
	memset(keyStates, 0, sizeof(keyStates));
	memset(hotKeyDownCounters, 0, sizeof(hotKeyDownCounters));

	// Always keep the empty key down.
	keyStates[0] = KEY_DOWN;

	for (auto x = 1; x < 255; x++)
	{
		auto listenKeys = reinterpret_cast<BYTE*>(hotKeys);

		for (auto i = 0; i < sizeof(hotKeys_); i++)
		{
			if (listenKeys[i] == x)
			{
				keyStates[x] = KEY_MONITOR;

				if (x >= VK_XINPUT_MIN_VALUE && x <= VK_XINPUT_MAX_VALUE)
				{
					bMonitorXInput = true;
				}

				break;
			}
		}
	}

	DWORD currentKey;
	INT x;

	while (WaitForSingleObject(hEvent, 0) != WAIT_OBJECT_0)
	{
		// Record the state of every key associated with a hotkey.
		for (x = 1; x < 255; x++)
		{
			// Skip XInput keys.
			if (x >= VK_XINPUT_MIN_VALUE && x <= VK_XINPUT_MAX_VALUE)
			{
				continue;
			}

			// Make sure we have to monitor this key.
			if (keyStates[x] & KEY_MONITOR)
			{
				if ((GetAsyncKeyState(x) & 0x8000) == 0)
				{
					// The key isn't down.
					keyStates[x] = KEY_MONITOR;
				}
				else
				{
					// The key is down.
					keyStates[x] |= KEY_DOWN;
				}
			}
		}

		if (bMonitorXInput)
		{
			FillXInputs(keyStates, dwLastXInputPacket);
		}

		auto ctrlDown = (keyStates[VK_CONTROL] & KEY_DOWN) != 0;
		auto altDown = (keyStates[VK_MENU] & KEY_DOWN) != 0;
		auto shiftDown = (keyStates[VK_SHIFT] & KEY_DOWN) != 0;

		for (x = 0; x < 512; x++)
		{
			currentKey = hotKeys[x];

			// Is this the end of the list?
			if (currentKey == 0)
			{
				break;
			}

			// Force a context switch every 10 key checks to keep the processor cool.
			if (x != 0 && x % 10 == 0)
			{
				Sleep(0);
			}

			auto hasCtrl = false;
			auto hasAlt = false;
			auto hasShift = false;

			auto isPressed = true;

			while (currentKey != 0)
			{
				auto partialKey = currentKey & 0xff;

				if ((keyStates[partialKey] & KEY_DOWN) == 0)
				{
					isPressed = false;
					break;
				}

				if (partialKey == VK_CONTROL)
				{
					hasCtrl = true;
				}

				if (partialKey == VK_MENU)
				{
					hasAlt = true;
				}

				if (partialKey == VK_SHIFT)
				{
					hasShift = true;
				}

				currentKey >>= 8;
			}

			// Make sure the modifiers match exactly. We don't 
			// want a CTRL+A input triggering an A hotkey!
			isPressed = isPressed && ctrlDown == hasCtrl && altDown == hasAlt && shiftDown == hasShift;

			if (!isPressed)
			{
				hotKeyDownCounters[x] = 0;
				continue;
			}
			
			auto consecutiveTimesDown = ++hotKeyDownCounters[x];

			if (consecutiveTimesDown == 255)
			{
				// Handle overflow.
				hotKeyDownCounters[x] = 23;
				consecutiveTimesDown = 23;
			}

			auto handler = monitor->handler_;

			if (handler == nullptr)
			{
				// If there is nothing monitoring the
				// keys, skip the calculations.
				continue;
			}

			if (consecutiveTimesDown == 1)
			{
				// The is the first time the hotkey was pressed.
				isPressed = true;
			}
			else if (consecutiveTimesDown <= 20)
			{
				// If the hotkey was down the last 20 
				// times, pretend it's not down at all.
				isPressed = false;
			}
			else
			{
				// If the hotkey was down for over 20
				// consecutive iterations, mark it as
				// down every 4 iterations.
				isPressed = consecutiveTimesDown % 4 == 0;
			}

			if (isPressed)
			{
				handler(hotKeys[x]);
			}
		}

		Sleep(55);
	}

	CloseHandle(hEvent);

	return 0;
}
