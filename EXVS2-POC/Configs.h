#pragma once
#include <string>

struct jvs_key_bind {
	int Test;
	int Start;
	int Service;
	int Up;
	int Left;
	int Down;
	int Right;
	int Button1;
	int Button2;
	int Button3;
	int Button4;
	int ArcadeButton1;
	int ArcadeButton2;
	int ArcadeButton3;
	int ArcadeButton4;
	int ArcadeStartButton;
};

struct config_struct {
	jvs_key_bind KeyBind;
	bool Windowed;
	bool UseDirectInput;
	std::string Serial;
	std::string PcbId;
	std::string TenpoRouter;
	std::string AuthServerIp;
	std::string IpAddress;
	std::string SubnetMask;
	std::string Gateway;
	std::string PrimaryDNS;
	std::string ServerAddress;
};