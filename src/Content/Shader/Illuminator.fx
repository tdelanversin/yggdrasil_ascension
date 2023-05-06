#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

struct X_Vector3
{
    float X;
    float Y;
    float Z;
    int WX;
    int WY;
};

struct Point3
{
    float X;
    float Y;
    float Z;
};

static const int indices[36] =
{
    /* bottom */ 0, 1, 2, 0, 2, 3,
    /* right  */ 2, 6, 3, 3, 6, 7,
    /* front  */ 1, 5, 2, 2, 5, 6,
    /* left   */ 0, 4, 1, 1, 4, 5,
    /* back   */ 0, 3, 7, 0, 7, 4,
    /* top    */ 5, 4, 6, 6, 4, 7
};

StructuredBuffer<Point3> Vertices;
StructuredBuffer<X_Vector3> Coords;
StructuredBuffer<Point3> Lights;

RWTexture2D<float4> Shade;

const float eps = 1.0e-7f;
int NumLights;
int NumCoords;
int NumVerts;

int RayIntersect(float3 origin, float3 direction, uint globalIDx)
{
    float len = length(direction);
    float3 dir = normalize(direction);
    
    for (int ind = 0; ind < NumVerts; ind += 8)
    {
        int index = 0;
        for (int i = 0; i < 36; i += 3)
        {
            Point3 p0x = Vertices[ind + indices[i]];
            Point3 p1x = Vertices[ind + indices[i + 1]];
            Point3 p2x = Vertices[ind + indices[i + 2]];
            
            float3 p0 = float3(p0x.X, p0x.Y, p0x.Z);
            float3 p1 = float3(p1x.X, p1x.Y, p1x.Z);
            float3 p2 = float3(p2x.X, p2x.Y, p2x.Z);
            
            // Find vectors for two edges sharing v[0]
            float3 edge1 = p1 - p0;
            float3 edge2 = p2 - p0;
            
            // remove backside
            float3 n = cross(edge1, edge2);
            n = normalize(n);
            if (dot(dir, n) > eps)
                continue;
            
            // Begin calculating determinant - also used to calculate U parameter
            float3 pvec = cross(dir, edge2);
            
            // If determinant is near zero, ray lies in plane of triangle
            float det = dot(edge1, pvec);
        
            if (det > -eps && det < eps)
                continue;
            
            //return false;
            float invDet = 1.0f / det;
            
            // Calculate distance from v[0] to ray origin
            float3 tvec = origin - p0;
            
            // Calculate U parameter and test bounds
            float u = dot(tvec, pvec) * invDet;
            if (u < 0.0 || u > 1.0)
                continue;
        
            // Prepare to test V parameter
            float3 qvec = cross(tvec, edge1);
        
            // Calculate V parameter and test bounds
            float v = dot(dir, qvec) * invDet;
            if (v < 0.0 || u + v > 1.0)
                continue;
        
            // Ray intersects triangle -> compute t
            float t = dot(edge2, qvec) * invDet;
        
            if (t >= eps && t <= len)
            {
                //hitPoint = origin + t * direction;
                //if (index == 2)
                //{
                //    TestVals[globalIDx].X = 1;
                //    TestVals[globalIDx].Y = 0;
                //    TestVals[globalIDx].Z = 0;
                //}
                //index++;
                return 1;
            }
        }
    }
    //hitPoint = new Vector3(0, 0, 0);
    return 0;
}

//================================================================================================
// Compute Shader
//================================================================================================
#define GroupSize 512

[numthreads(GroupSize, 1, 1)]
void CS(uint3 localID : SV_GroupThreadID, uint3 groupID : SV_GroupID,
        uint localIndex : SV_GroupIndex, uint3 globalID : SV_DispatchThreadID)
{
    uint c = globalID.x;
    uint2 ind = uint2(Coords[c].WX, Coords[c].WY);
    
    for (int l = 0; l < NumLights; l++)
    {
        float3 lightPos = float3(Lights[l].X, Lights[l].Y, Lights[l].Z);
        float3 pos = float3(Coords[c].X, Coords[c].Y, Coords[c].Z);
        float3 direction = pos - lightPos;
        if (RayIntersect(lightPos, direction, c) == 0)
        {
            Shade[ind].a = 0.0f;
            return;
        }
    }
    Shade[ind].a = 1.0f;
    
    //Shade[ind] = float4(0.5f, 0.5f, 0.5f, 1.0f);

}

//================================================================================================
// Techniques
//================================================================================================
technique Tech0
{
    pass Pass0
    {
        ComputeShader = compile cs_5_0 CS();
    }
};
