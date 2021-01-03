// 通过近岸距离、世界坐标、噪声贴图，得到用作Color增量的float值
float Foam(float shore, float2 worldXZ, sampler2D noiseTex)
{
	shore = sqrt(shore) * 0.9; // 让近岸的泡沫更大，通过让y参数 往里密集化

	// 加入噪声扰动
	float2 noiseUV = worldXZ + _Time.y * 0.25; // 运动采样
	float4 noise = tex2D(noiseTex, noiseUV * 0.015);

	float distortion1 = noise.x * (1 - shore); // 越靠岸，distortion越小
	float foam1 = sin((shore + distortion1) * 10 - _Time.y); // 波纹朝外运动
	foam1 *= foam1;

	float distortion2 = noise.y * (1 - shore);
	float foam2 = sin((shore + distortion2) * 10 + _Time.y + 2); // 朝内运动的波纹
	foam2 *= foam2 * 0.7; // 弱化

	return max(foam1, foam2) * shore; // 越靠岸越大
}

float Waves(float2 worldXZ, sampler2D noiseTex)
{
	float2 uv1 = worldXZ;
	uv1.y += _Time.y;
	float4 noise1 = tex2D(noiseTex, uv1 * 0.025);

	float2 uv2 = worldXZ;
	uv2.x += _Time.y;
	float4 noise2 = tex2D(noiseTex, uv2 * 0.025);

	float blendWave = sin(
		(worldXZ.x + worldXZ.x) * 0.1 +
		(noise1.y + noise2.z) + _Time.y
	); // 让波纹斜向运动
	blendWave *= blendWave;

	float waves =
		lerp(noise1.z, noise1.w, blendWave) +
		lerp(noise2.x, noise2.y, blendWave);
	return smoothstep(0.75, 2, waves);
}