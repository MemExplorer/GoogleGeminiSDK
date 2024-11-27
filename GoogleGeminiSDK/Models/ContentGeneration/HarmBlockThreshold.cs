﻿namespace GoogleGeminiSDK.Models.ContentGeneration;

// ReSharper disable InconsistentNaming
public enum HarmBlockThreshold
{
	HARM_BLOCK_THRESHOLD_UNSPECIFIED,
	BLOCK_LOW_AND_ABOVE,
	BLOCK_MEDIUM_AND_ABOVE,
	BLOCK_ONLY_HIGH,
	BLOCK_NONE,
	OFF
}