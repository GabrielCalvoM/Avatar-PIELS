using System;
using System.Collections.Generic;

[Serializable]
public class KeyframeData
{
    public float second;
    public PoseData pose;
}

[Serializable]
public class AnimationSaveRequest
{
    public string animationName;
    public List<KeyframeData> keyframes;
}

[Serializable]
public class AnimationResponse
{
    public string animationName;
    public List<KeyframeData> keyframes;
}

[Serializable]
public class AnimationNamesResponse
{
    public List<string> animationNames;
}
