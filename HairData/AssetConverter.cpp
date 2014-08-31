//--------------------------------------------------------------------------------------
// AssetConverter.cpp 
//
// File for converting the hair assets from .tfx files to a binary .tfxb file
// 
//
// Copyright 2014 ADVANCED MICRO DEVICES, INC.  All Rights Reserved.
//
// AMD is granting you permission to use this software and documentation (if
// any) (collectively, the "Materials") pursuant to the terms and conditions
// of the Software License Agreement included with the Materials.  If you do
// not have a copy of the Software License Agreement, contact your AMD
// representative for a copy.
// You agree that you will not reverse engineer or decompile the Materials,
// in whole or in part, except as allowed by applicable law.
//
// WARRANTY DISCLAIMER: THE SOFTWARE IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND.  AMD DISCLAIMS ALL WARRANTIES, EXPRESS, IMPLIED, OR STATUTORY,
// INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE, TITLE, NON-INFRINGEMENT, THAT THE SOFTWARE
// WILL RUN UNINTERRUPTED OR ERROR-FREE OR WARRANTIES ARISING FROM CUSTOM OF
// TRADE OR COURSE OF USAGE.  THE ENTIRE RISK ASSOCIATED WITH THE USE OF THE
// SOFTWARE IS ASSUMED BY YOU.
// Some jurisdictions do not allow the exclusion of implied warranties, so
// the above exclusion may not apply to You. 
// 
// LIMITATION OF LIABILITY AND INDEMNIFICATION:  AMD AND ITS LICENSORS WILL
// NOT, UNDER ANY CIRCUMSTANCES BE LIABLE TO YOU FOR ANY PUNITIVE, DIRECT,
// INCIDENTAL, INDIRECT, SPECIAL OR CONSEQUENTIAL DAMAGES ARISING FROM USE OF
// THE SOFTWARE OR THIS AGREEMENT EVEN IF AMD AND ITS LICENSORS HAVE BEEN
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.  
// In no event shall AMD's total liability to You for all damages, losses,
// and causes of action (whether in contract, tort (including negligence) or
// otherwise) exceed the amount of $100 USD.  You agree to defend, indemnify
// and hold harmless AMD and its licensors, and any of their directors,
// officers, employees, affiliates or agents from and against any and all
// loss, damage, liability and other expenses (including reasonable attorneys'
// fees), resulting from Your use of the Software or violation of the terms and
// conditions of this Agreement.  
//
// U.S. GOVERNMENT RESTRICTED RIGHTS: The Materials are provided with "RESTRICTED
// RIGHTS." Use, duplication, or disclosure by the Government is subject to the
// restrictions as set forth in FAR 52.227-14 and DFAR252.227-7013, et seq., or
// its successor.  Use of the Materials by the Government constitutes
// acknowledgement of AMD's proprietary rights in them.
// 
// EXPORT RESTRICTIONS: The Materials may be subject to export restrictions as
// stated in the Software License Agreement.
//
//--------------------------------------------------------------------------------------

#include "AssetConverter.h"

#include <iostream>
#include <fstream>
#include <sstream>
#include <algorithm>
#include <tchar.h>

using namespace std;
using namespace DirectX;

#define MAX_STRING_SIZE 256

CAssetLoader g_AssetLoader;

int StringTokenizer(const string&, const string&, vector<string>&, bool);

//--------------------------------------------------------------------------------------
//
// Main
//
// Usage: assetconverter hairfilename1.tfx hairfilename2.tfx ... hairfilename4.tfx binaryfilename.tfxb
//
//--------------------------------------------------------------------------------------
int _tmain(int argc, _TCHAR* argv[])
{
	if (argc == 1)
	{
		printf("\nUsage: asssetconverter inputfile1.tfx ... inputfile4.tfx outputfile.tfxb");
		return 1;
	}

	if (argc > (HAIR_MAX_SECTIONS + 2))
	{
		printf("\nToo many parameters. From 1 to 4 hair files can be used.\nUsage: asssetconverter inputfile1.tfx ... inputfile4.tfx outputfile.tfxb");
		return 1;
	}

	if (argc < 3)
	{
		printf("\nToo few parameters. 1 to 4 hair files and the output file must be specified.\nUsage: asssetconverter inputfile1.tfx ... inputfile4.tfx outputfile.tfxb");
		return 1;
	}

	int numfiles = argc - 2;
	size_t numConverted;
	char filename[MAX_STRING_SIZE];
	g_AssetLoader.m_HairAsset.m_MaxNumOfVerticesInStrand = 2; // minimum value

	for (int i = 0; i < numfiles; i++)
	{
		wcstombs_s(&numConverted, filename, (size_t)MAX_STRING_SIZE, argv[i+1], (size_t)MAX_STRING_SIZE);
		if(!g_AssetLoader.LoadAppend(filename, i, THREAD_GROUP_SIZE))
		{
			printf("\nFailed to load hair file");
			return 1;
		}
	}

	g_AssetLoader.GenerateFollowHairs();
	g_AssetLoader.ProcessVertices();	

	// save out the binary file 
	wcstombs_s(&numConverted, filename, (size_t)MAX_STRING_SIZE, argv[argc-1], (size_t)MAX_STRING_SIZE);
	if (!g_AssetLoader.WriteBinaryFile(filename))
	{
		printf("\nFailed to write output.\nEnsure output file is not read-only");
		return 1;
	}

	return 0;
}



//--------------------------------------------------------------------------------------
//
// Constructor
//
//--------------------------------------------------------------------------------------
CHairAsset::CHairAsset(void) : m_bGuideHair(false)
{
   
}

//--------------------------------------------------------------------------------------
//
// Destructor
//
//--------------------------------------------------------------------------------------
CHairAsset::~CHairAsset(void)
{
    Clear();
}

//--------------------------------------------------------------------------------------
//
// Clear
//
// Clears the vertex array
//
//--------------------------------------------------------------------------------------
void CHairAsset::Clear()
{
    m_VertexArray.clear();
}

//--------------------------------------------------------------------------------------
//
// ConstructAsset
//
// Initializes the local, global transformations and reference vector for the hair strand
//
//--------------------------------------------------------------------------------------
void CHairAsset::ConstructAsset()
{   
    // vertex 0
    {
        HairVertex& vert_i = m_VertexArray[0];
        HairVertex& vert_i_plus_1 = m_VertexArray[1];

        const CVector3D vec = vert_i_plus_1.m_Pos - vert_i.m_Pos;
        CVector3D vecX = vec.NormalizeOther();

        CVector3D vecZ = vecX.Cross(CVector3D(1.0, 0, 0));

        if ( vecZ.LengthSqr() < 0.0001 )
        {
            vecZ = vecX.Cross(CVector3D(0, 1.0f, 0));
        }

        vecZ.Normalize();
        CVector3D vecY = vecZ.Cross(vecX).Normalize();

        CMatrix33 rotL2W;

        rotL2W(0, 0) = vecX.m_X;	rotL2W(0, 1) = vecY.m_X;		rotL2W(0, 2) = vecZ.m_X;
        rotL2W(1, 0) = vecX.m_Y;	rotL2W(1, 1) = vecY.m_Y;		rotL2W(1, 2) = vecZ.m_Y;
        rotL2W(2, 0) = vecX.m_Z;	rotL2W(2, 1) = vecY.m_Z;		rotL2W(2, 2) = vecZ.m_Z;

        vert_i.m_LocalTransform.GetRotation() = rotL2W;
        vert_i.m_LocalTransform.GetTranslation() = vert_i.m_Pos;
        vert_i.m_GlobalTransform = vert_i.m_LocalTransform; // For vertex 0, local and global transforms are the same. 
    }

    // vertex 1 through n-1
    for (int i = 1 ; i < (int)m_VertexArray.size(); i++ )
    {
        HairVertex& vert_i_minus_1 = m_VertexArray[i-1];
        HairVertex& vert_i = m_VertexArray[i];
        
        CVector3D vec = vert_i.m_Pos - vert_i_minus_1.m_Pos;
        vec = vert_i_minus_1.m_GlobalTransform.GetRotation().InverseOther() * vec;

        CVector3D vecX = vec.NormalizeOther();
    
        CVector3D X = CVector3D(1.0f, 0, 0);
        CVector3D rotAxis = X.Cross(vecX);
        float angle = acos(X.Dot(vecX));

        if ( abs(angle) < 0.001 || rotAxis.LengthSqr() < 0.001 )
        {
            vert_i.m_LocalTransform.GetRotation().SetIdentity();
        }
        else
        {
            rotAxis.Normalize();
            CQuaternion rot = CQuaternion(rotAxis, angle);
            vert_i.m_LocalTransform.GetRotation() = rot;
        }

        vert_i.m_LocalTransform.GetTranslation() = vec;
        vert_i.m_GlobalTransform = vert_i_minus_1.m_GlobalTransform * vert_i.m_LocalTransform;
        vert_i.m_RefVector = vert_i.m_LocalTransform.GetTranslation();
    }
}

//--------------------------------------------------------------------------------------
//
// Constructor
//
//--------------------------------------------------------------------------------------
CAssetLoader::CAssetLoader(void) 
{
	m_HairAsset.m_pHairStrandType = NULL;
	m_HairAsset.m_pRefVectors = NULL;
	m_HairAsset.m_pTriangleVertices = NULL;
	m_HairAsset.m_pGlobalRotations = NULL;
	m_HairAsset.m_pLocalRotations = NULL;
	m_HairAsset.m_pVertices = NULL;
	m_HairAsset.m_pTangents = NULL;
	m_HairAsset.m_pThicknessCoeffs = NULL;
	m_HairAsset.m_pRestLengths = NULL;
	m_HairAsset.m_pFollowRootOffset = NULL;
	m_HairAsset.m_NumFollowHairsPerOneGuideHair = 4;
	m_HairAsset.m_NumGuideHairStrands = 0;
	m_HairAsset.m_MaxNumOfVerticesInStrand = 0;
	m_scale = 1.0;
	m_rotation = XMFLOAT3(0, 0, 0);
	m_translation = XMFLOAT3(0, 0, 0);;
	m_bothEndsImmovable = 0;
	m_maxRadiusAroundGuideHair = 0.5;

}

//--------------------------------------------------------------------------------------
//
// Destructor
//
//--------------------------------------------------------------------------------------
CAssetLoader::~CAssetLoader(void)
{
	Clear();
}

//--------------------------------------------------------------------------------------
//
// DestroyAll
//
// Deletes member variables
//
//--------------------------------------------------------------------------------------
void CAssetLoader::DestroyAll()
{
    if ( m_HairAsset.m_pVertices )
    {
        delete [] m_HairAsset.m_pVertices;
        m_HairAsset.m_pVertices = NULL;
    }

    if ( m_HairAsset.m_pTangents )
    {
        delete [] m_HairAsset.m_pTangents;
        m_HairAsset.m_pTangents = NULL;
    }

    if ( m_HairAsset.m_pHairStrandType )
    {
        delete [] m_HairAsset.m_pHairStrandType;
        m_HairAsset.m_pHairStrandType = NULL;
    }

	if ( m_HairAsset.m_pLocalRotations )
	{
		delete [] m_HairAsset.m_pLocalRotations;
		m_HairAsset.m_pLocalRotations = NULL;
	}

	if ( m_HairAsset.m_pGlobalRotations )
	{
		delete [] m_HairAsset.m_pGlobalRotations;
		m_HairAsset.m_pGlobalRotations = NULL;
	}

	if ( m_HairAsset.m_pRefVectors )
	{
		delete [] m_HairAsset.m_pRefVectors;
		m_HairAsset.m_pRefVectors = NULL;
	}

	if ( m_HairAsset.m_pTriangleVertices )
	{
		delete [] m_HairAsset.m_pTriangleVertices;
		m_HairAsset.m_pTriangleVertices = NULL;
	}

	if ( m_HairAsset.m_pThicknessCoeffs )
	{
		delete [] m_HairAsset.m_pThicknessCoeffs;
		m_HairAsset.m_pThicknessCoeffs = NULL;
	}
	
	if ( m_HairAsset.m_pRestLengths )
	{
		delete [] m_HairAsset.m_pRestLengths;
		m_HairAsset.m_pRestLengths = NULL;
	}

	if ( m_HairAsset.m_pFollowRootOffset )
	{
		delete [] m_HairAsset.m_pFollowRootOffset;
		m_HairAsset.m_pFollowRootOffset = NULL;
	}

}

//--------------------------------------------------------------------------------------
//
// GetTangentVectors
//
// Create two arbitrary tangent vectors (t0 and t1) perpendicular to the input normal vector (n).
//
//--------------------------------------------------------------------------------------
void GetTangentVectors(const CVector3D& n, CVector3D& t0, CVector3D& t1)
{
	if ( fabsf(n[2]) > 0.707f ) 
	{
		float a = n[1]*n[1] + n[2]*n[2];
		float k = 1.0f/sqrtf(a);
		t0[0] = 0;
		t0[1] = -n[2]*k;
		t0[2] = n[1]*k;

		t1[0] = a*k;
		t1[1] = -n[0]*t0[2];
		t1[2] = n[0]*t0[1];
	}
	else 
	{
		float a = n[0]*n[0] + n[1]*n[1];
		float k = 1.0f/sqrtf(a);
		t0[0] = -n[1]*k;
		t0[1] = n[0]*k;
		t0[2] = 0;

		t1[0] = -n[2]*t0[1];
		t1[1] = n[2]*t0[0];
		t1[2] = a*k;
	}
}

#define GET_NEXT_LINE_OF_TOKENS \
		sTokens.clear(); \
		getline(inFile, sLine); \
        StringTokenizer(sLine, string(" "), sTokens, false); \
		iter = sTokens.begin(); \
		sToken = *(iter);

//--------------------------------------------------------------------------------------
//
// LoadAppend
//
// Reads in a file of hair data and appends it to the list of hair strands
//
//--------------------------------------------------------------------------------------
bool CAssetLoader::LoadAppend(const char* filename, int groupId, int threadGoupSize)
{
    ifstream inFile(filename);
    if (!inFile.good())
    {
        return false;
    }

    string sLine;
    vector<string> sTokens;
    vector <string>::iterator iter;
    string sToken; 

	// default values
	m_scale = 1.0;
	m_rotation = XMFLOAT3(0, 0, 0);;
	m_translation = XMFLOAT3(0, 0, 0);;
	m_bothEndsImmovable = false;
	m_maxRadiusAroundGuideHair = 0.5;
	m_HairAsset.m_NumFollowHairsPerOneGuideHair = 4;

	GET_NEXT_LINE_OF_TOKENS
	// read the version number
	if (sToken == "version")
	{
		sToken = *(++iter);
		float version = (float)atof(sToken.c_str());
		if (version >= 2.0)
		{
			float x,y,z;
			// m_scale, m_rotation, m_translation
			GET_NEXT_LINE_OF_TOKENS
			assert(sToken == "scale");
			sToken = *(++iter);
			m_scale = (float)atof(sToken.c_str());
			GET_NEXT_LINE_OF_TOKENS
			assert(sToken == "rotation");
			sToken = *(++iter);
			x = (float)atof(sToken.c_str());
			sToken = *(++iter);
			y = (float)atof(sToken.c_str());
			sToken = *(++iter);
			z = (float)atof(sToken.c_str());
			m_rotation = XMFLOAT3(x, y, z);
			GET_NEXT_LINE_OF_TOKENS
			assert(sToken == "translation");
			sToken = *(++iter);
			x = (float)atof(sToken.c_str());
			sToken = *(++iter);
			y = (float)atof(sToken.c_str());
			sToken = *(++iter);
			z = (float)atof(sToken.c_str());
			m_translation = XMFLOAT3(x, y, z);
			// both ends immovable
			GET_NEXT_LINE_OF_TOKENS
			assert(sToken == "bothEndsImmovable");
			sToken = *(++iter);
			int val = atoi(sToken.c_str());
			if (val != 0)
				m_bothEndsImmovable = true;
			else
				m_bothEndsImmovable = false;

			// max num vertices in strand
			GET_NEXT_LINE_OF_TOKENS
			assert(sToken == "maxNumVerticesInStrand");
			sToken = *(++iter);
			int maxVertsInStrand = atoi(sToken.c_str());
			m_HairAsset.m_MaxNumOfVerticesInStrand = max(maxVertsInStrand, m_HairAsset.m_MaxNumOfVerticesInStrand);

			// num follow hairs per guide hair
			GET_NEXT_LINE_OF_TOKENS
			assert(sToken == "numFollowHairsPerGuideHair");
			sToken = *(++iter);
			m_HairAsset.m_NumFollowHairsPerOneGuideHair = atoi(sToken.c_str());

			// max radius around guide hair
			GET_NEXT_LINE_OF_TOKENS
			assert(sToken == "maxRadiusAroundGuideHair");
			sToken = *(++iter);
			m_maxRadiusAroundGuideHair = (float)atof(sToken.c_str());
		}
		// num strands
		GET_NEXT_LINE_OF_TOKENS
	}

	// num strands
   assert(sToken == "numStrands");
    ++iter;
    sToken = *(iter);
    int numStrands = atoi(sToken.c_str());

    getline(inFile, sLine); // is sorted 1
	std::vector<CHairAsset*> tempHairs;
    
	for ( int strand = 0; strand < numStrands; strand++ )
    {
        CHairAsset* pHair = new CHairAsset();
		pHair->SetGuideHair(true);

        // numVerts
        sTokens.clear(); 
		sTokens.reserve(10);
        getline(inFile, sLine); // strand 0 numVerts 25 texcoord 0.000000 0.522833
        StringTokenizer(sLine, string(" "), sTokens, false);
        iter = sTokens.begin() + 3;
        sToken = *(iter);
        int numVerts = atoi(sToken.c_str());		
        pHair->GetVertexArray().reserve(numVerts);

        for ( int vertex = 0; vertex < numVerts; vertex++ )
        {
            getline(inFile, sLine);
            sTokens.clear(); 
			sTokens.reserve(10);
            int numFound = StringTokenizer(sLine, string(" "), sTokens, false);

            if ( numFound == 0 )
                continue;

            iter = sTokens.begin();

            if ( (*iter) == "-1.#INF" )
                continue;

            CVector3D pnt;
            
            // x
            sToken = (*iter);			
            pnt.m_X = (float)atof(sToken.c_str());

            // y
            ++iter;
            sToken = (*iter);			
            pnt.m_Y = (float)atof(sToken.c_str());

            // z
            ++iter;
            sToken = (*iter);			
            pnt.m_Z = (float)atof(sToken.c_str());
            
            HairVertex vert;
            vert.m_Pos.Set(pnt.m_X, pnt.m_Y, pnt.m_Z);

			// In some cases, two end vertices in both ends of strand are needed to be immovable. 
            if ( m_bothEndsImmovable )
            {
                if ( vertex == 0 || vertex == 1 || vertex == numVerts -1 || vertex == numVerts - 2 )
                    vert.m_InvMass = 0;
                else
                    vert.m_InvMass = 1.0f;
            }
            else
            {
                if ( vertex == 0 || vertex == 1 )
                    vert.m_InvMass = 0;
                else
                    vert.m_InvMass = 1.0f;
            }

			// Limit the maximum number of vertices per strand.
			if ( vertex < m_HairAsset.m_MaxNumOfVerticesInStrand )	
			{
				pHair->GetVertexArray().push_back(vert);
			}
        }

		pHair->m_GroupID = groupId;
		
        // add the new hair into tempHairs
        if ( pHair->GetVertexArray().size() > 2 )
		{
            tempHairs.push_back(pHair);		
		}
        else
            delete pHair;
    }

    inFile.close();
	
	// Make the number of strands a multiple of thread group size.
	// This is an easy way to avoid branching in compute shader.
	unsigned int loadedNumStrands = (unsigned int)tempHairs.size();
	unsigned int numOfDelete = loadedNumStrands % threadGoupSize;

	for ( unsigned int i = 0; i < numOfDelete; i++ )
	{
		std::vector<CHairAsset*>::iterator iter = --tempHairs.end();

		if ( (*iter) )
			delete (*iter);

		tempHairs.pop_back();
	}
	
	m_HairAsset.m_NumGuideHairStrands += (int)tempHairs.size();

	for ( size_t i = 0; i < tempHairs.size(); i++ )
	{
		m_Hairs.push_back(tempHairs[i]);

		for ( int j = 0; j < m_HairAsset.m_NumFollowHairsPerOneGuideHair; j++ )
			m_Hairs.push_back(NULL); // add placeholder for follow hair
	}

    return true;	
}

//--------------------------------------------------------------------------------------
//
// GetRandom
//
// random number generator
//
//--------------------------------------------------------------------------------------
float GetRandom(float Min, float Max)
{
    return ((float(rand()) / float(RAND_MAX)) * (Max - Min)) + Min;
}

//--------------------------------------------------------------------------------------
//
// GenerateFollowHairs
//
// Generates the slave hairs
//
//--------------------------------------------------------------------------------------
void CAssetLoader::GenerateFollowHairs()
{	
	assert(m_HairAsset.m_NumFollowHairsPerOneGuideHair >= 0);

	if ( m_HairAsset.m_NumFollowHairsPerOneGuideHair == 0 )
		return;

	std::vector<CHairAsset*> tempHairs;
	tempHairs.reserve(m_HairAsset.m_NumGuideHairStrands);

	for ( int i = 0; i < (int)m_Hairs.size(); i++ )
	{
		if ( m_Hairs[i] && m_Hairs[i]->IsGuideHair() )
			tempHairs.push_back(m_Hairs[i]);
	}

	m_Hairs.clear();
	m_Hairs.resize(m_HairAsset.m_NumGuideHairStrands + m_HairAsset.m_NumGuideHairStrands * (m_HairAsset.m_NumFollowHairsPerOneGuideHair));

	// randomize the hair strands for LOD density
	random_shuffle(tempHairs.begin(), tempHairs.end());

	for ( int i = 0; i < (int)tempHairs.size(); i++ )
	{
		m_Hairs[i*(m_HairAsset.m_NumFollowHairsPerOneGuideHair+1)] = tempHairs[i];
	}

	// Generate follow hairs from guide hairs
	for ( int i = 0; i < m_HairAsset.m_NumGuideHairStrands; i++ )
	{
		const CHairAsset* pGuideHair = m_Hairs[i*(m_HairAsset.m_NumFollowHairsPerOneGuideHair+1)];

		for ( int j = 0; j < m_HairAsset.m_NumFollowHairsPerOneGuideHair; j++ )
		{
			CHairAsset* pNewFollowHair = new CHairAsset();
			pNewFollowHair->SetGuideHair(false);
			pNewFollowHair->m_GroupID = pGuideHair->m_GroupID;
			CVector3D v01 = pGuideHair->GetVertexArray()[1].m_Pos - pGuideHair->GetVertexArray()[0].m_Pos;
			v01.Normalize();

			// Find two orthogonal unit tangent vectors to v01
			CVector3D t0, t1;
			GetTangentVectors(v01, t0, t1);
			CVector3D offset = GetRandom(-m_maxRadiusAroundGuideHair, m_maxRadiusAroundGuideHair)*t0 + GetRandom(-m_maxRadiusAroundGuideHair, m_maxRadiusAroundGuideHair)*t1;

			for ( int k = 0; k < (int)pGuideHair->GetVertexArray().size(); k++ )
			{
				HairVertex vert;
				float factor = 5.0f*(float)k/(float)pGuideHair->GetVertexArray().size() + 1.0f; // 5.0 and 1.0 should match the in UpdateFollowHairVertices in TressFXSimulation.hlsl
				vert.m_Pos =  pGuideHair->GetVertexArray()[k].m_Pos + offset*factor;
				vert.m_InvMass = pGuideHair->GetVertexArray()[k].m_InvMass;
				pNewFollowHair->GetVertexArray().push_back(vert);
			}

			m_Hairs[i*(m_HairAsset.m_NumFollowHairsPerOneGuideHair+1)+j+1] = pNewFollowHair;			
		}
	}
}

//--------------------------------------------------------------------------------------
//
// Clear
//
// Clear the array of hair strands
//
//--------------------------------------------------------------------------------------
void CAssetLoader::Clear()
{
    for ( int i = 0; i < (int)m_Hairs.size(); i++ )
    {
        if ( m_Hairs[i] )
            delete m_Hairs[i];
    }

    m_Hairs.clear();

	m_HairAsset.m_LineIndices.clear();
	m_HairAsset.m_Triangleindices.clear();

	DestroyAll();
}

//--------------------------------------------------------------------------------------
//
// ComputeStrandTangent
//
// Calculates the tangent value for each vertices of the strand
//
//--------------------------------------------------------------------------------------
void CAssetLoader::ComputeStrandTangent(vector<StrandVertex>& strand)
{
    int numVertices = (int)strand.size();

    // Calculate the tangent value for the first vertex of the strand 
    XMVECTOR tangent = XMLoadFloat3(&strand[1].position) - XMLoadFloat3(&strand[0].position);
    tangent = XMVector3Normalize(tangent);
    XMStoreFloat3( &strand[0].tangent, tangent );
    
    for (int vertex = 1; vertex < numVertices-1; vertex++) 
    {
        XMVECTOR tangent_pre = XMLoadFloat3(&strand[vertex].position) - XMLoadFloat3(&strand[vertex-1].position);
        tangent_pre = XMVector3Normalize(tangent_pre);

        XMVECTOR tangent_next = XMLoadFloat3(&strand[vertex+1].position) - XMLoadFloat3(&strand[vertex].position);
        tangent_next = XMVector3Normalize(tangent_next);

        tangent = tangent_pre + tangent_next;
        tangent = XMVector3Normalize(tangent);

        XMStoreFloat3( &strand[vertex].tangent, tangent );
    }

    // Calculate the tangent value for the last vertex of the strand 
    tangent = XMLoadFloat3(&strand[numVertices-1].position) - XMLoadFloat3(&strand[numVertices-2].position);
    tangent = XMVector3Normalize(tangent);
    XMStoreFloat3( &strand[numVertices-1].tangent, tangent );
}

//--------------------------------------------------------------------------------------
//
// ComputeDistanceToRoot
//
// Calculates the parametric distance to the root for each vertex in the strand
//
// z value: [0, 1] root:0, tip:1
//
//--------------------------------------------------------------------------------------
float CAssetLoader::ComputeDistanceToRoot(vector<StrandVertex>& strand)
{
    int numVertices = (int)strand.size();
    float strandLength = 0;
    strand[0].texcoord.z = 0;
    for (int i=1; i<numVertices; i++)
    {
        XMVECTOR vec = XMLoadFloat3(&strand[i].position) - XMLoadFloat3(&strand[i-1].position);
        float disSeg = XMVectorGetX( XMVector3Length(vec) );
        strand[i].texcoord.z = strand[i-1].texcoord.z + disSeg;
        strandLength += disSeg;
    }
    for (int i=0; i<numVertices; i++)
    {
        strand[i].texcoord.z /= strandLength;
    }

    return strandLength;
}

//--------------------------------------------------------------------------------------
//
// ScaleRotateTranslate
//
// Affine transforms on the mesh
//
//--------------------------------------------------------------------------------------
void CAssetLoader::ScaleRotateTranslate()
{
    // Scale, rotate, translate and calculate bounding box and sphere
    BBox bBox;

    // Translation
    for(int i=0; i < int(GetHairs().size()); i++)
    {
        std::vector<HairVertex>& vertices = GetHairs().at(i)->GetVertexArray();

        for ( int j = 0; j < (int)vertices.size(); j++ )
        {

            StrandVertex* pVertex = &(m_HairStrands[i][j]);

            // scale
            pVertex->position.x *= m_scale;
            pVertex->position.y *= m_scale;
            pVertex->position.z *= m_scale;

            // rotation
            XMMATRIX rotateMat = XMMatrixRotationRollPitchYaw(m_rotation.x, m_rotation.y, m_rotation.z);
            XMVECTOR position = XMVector3TransformCoord(XMVectorSet(pVertex->position.x, pVertex->position.y, pVertex->position.z, 1.0f), rotateMat);

            // translation
            position += XMVectorSet(m_translation.x, m_translation.y, m_translation.z, 0.0f);
            XMStoreFloat3(&pVertex->position, position);

            // also rotate the tangent
            XMVECTOR tangent = XMVector3TransformNormal(XMVectorSet(pVertex->tangent.x, pVertex->tangent.y, pVertex->tangent.z, 0.0f), rotateMat);
            XMStoreFloat3(&pVertex->tangent, tangent);

            // update the BBox
            bBox = Union(bBox, Float3(pVertex->position));
        }
    }
        
    Float3 c; float radius;
    bBox.BoundingSphere(&c, &radius);
    m_HairAsset.m_bSphere.center = XMFLOAT3(c.x, c.y, c.z);
    m_HairAsset.m_bSphere.radius = radius;
}

//--------------------------------------------------------------------------------------
//
// ProcessVertices
//
// After all of the vertices have been loaded ProcessVertices is called to create the
// associated data with the hair vertices, which includes attributes like tangents,strand
// length, and transformations. Also the hair type is stored with each vertex which 
// allows different simulation parameters for different sections of the hair.
//
//--------------------------------------------------------------------------------------
void CAssetLoader::ProcessVertices()
{
	m_HairAsset.m_NumTotalHairVertices = 0;
	m_HairAsset.m_NumGuideHairVertices = 0;

	// count vertices
	for ( int i = 0; i < (int)m_Hairs.size(); i++ )
	{
		m_HairAsset.m_NumTotalHairVertices += (int)m_Hairs[i]->GetVertexArray().size();

		if ( m_Hairs[i]->IsGuideHair() )
			m_HairAsset.m_NumGuideHairVertices += (int)m_Hairs[i]->GetVertexArray().size();
	}

	// construct reference vectors and reference frames
    for ( int i = 0; i < (int)m_Hairs.size(); i++ )
    {
        m_Hairs[i]->ConstructAsset();
    }
		
	m_HairAsset.m_NumTotalHairStrands = (int)GetHairs().size();

    m_HairAsset.m_pVertices = new XMFLOAT4[m_HairAsset.m_NumTotalHairVertices];
    m_HairAsset.m_pHairStrandType = new int[m_HairAsset.m_NumTotalHairStrands];  
    m_HairStrands.clear();
    m_HairStrands.resize(m_HairAsset.m_NumTotalHairStrands);
    m_HairAsset.m_pTangents = new XMFLOAT4[m_HairAsset.m_NumTotalHairVertices];
    int indexTang = 0;
    
	// Initialize the hair strands and compute tangents
    for ( int i=0; i < m_HairAsset.m_NumTotalHairStrands; i++ )
    {
        int numVerts = int(GetHairs().at(i)->GetVertexArray().size());
        m_HairStrands[i].resize(numVerts);

		for( int v=0; v < numVerts; v++ )
        {
            XMFLOAT3 pos;
            pos.x = GetHairs().at(i)->GetVertexArray().at(v).m_Pos.m_X;
            pos.y = GetHairs().at(i)->GetVertexArray().at(v).m_Pos.m_Y;
            pos.z = GetHairs().at(i)->GetVertexArray().at(v).m_Pos.m_Z;

            m_HairStrands[i][v].position = pos;
        }

        ComputeStrandTangent(m_HairStrands[i]);
		ComputeDistanceToRoot(m_HairStrands[i]);

        for( int v=0; v < numVerts; v++ )
        {
            m_HairAsset.m_pTangents[indexTang].x = m_HairStrands[i][v].tangent.x;
            m_HairAsset.m_pTangents[indexTang].y = m_HairStrands[i][v].tangent.y;
            m_HairAsset.m_pTangents[indexTang].z = m_HairStrands[i][v].tangent.z;

            indexTang++;
        }
    }

    m_HairAsset.m_pRestLengths = new float[m_HairAsset.m_NumTotalHairVertices];
	int index = 0;

	// Calculate rest lengths
	for ( int i = 0; i < m_HairAsset.m_NumTotalHairStrands; i++ )
	{
		for ( int j = 0; j < (int)m_HairStrands[i].size()-1; j++ )
		{
			m_HairAsset.m_pRestLengths[index++] = XMVectorGetX(XMVector3Length(XMLoadFloat3(&m_HairStrands[i][j].position) - XMLoadFloat3(&m_HairStrands[i][j+1].position)));
		}

		// Since number of edges are one less than number of vertices in hair strand, below line acts as a placeholder. 
		m_HairAsset.m_pRestLengths[index++] = 0;
	}

	assert(index == m_HairAsset.m_NumTotalHairVertices);

    m_HairAsset.m_pRefVectors = new XMFLOAT4[m_HairAsset.m_NumTotalHairVertices];
    m_HairAsset.m_pGlobalRotations = new XMFLOAT4[m_HairAsset.m_NumTotalHairVertices];
	m_HairAsset.m_pLocalRotations = new XMFLOAT4[m_HairAsset.m_NumTotalHairVertices];
    m_HairAsset.m_pTriangleVertices = new StrandVertex[m_HairAsset.m_NumTotalHairVertices];
    m_HairAsset.m_pThicknessCoeffs = new float[m_HairAsset.m_NumTotalHairVertices];
	m_HairAsset.m_pFollowRootOffset = new XMFLOAT4[m_HairAsset.m_NumTotalHairStrands];
    m_HairAsset.m_LineIndices.reserve(m_HairAsset.m_NumTotalHairVertices * 2);
    m_HairAsset.m_Triangleindices.reserve(m_HairAsset.m_NumTotalHairVertices * 6);
    int id=0;
    index = 0;

	CHairAsset* pGuideHair = NULL;
	int indexGuideHairStrand = -1; 

	// initialize the remainder of the hair data
    for ( int i = 0; i < m_HairAsset.m_NumTotalHairStrands; i++ )
    {
		int vertCount = (int)GetHairs().at(i)->GetVertexArray().size();

        for ( int j = 0; j < vertCount - 1; j++ )
        {
			// line indices
            m_HairAsset.m_LineIndices.push_back(id);
            m_HairAsset.m_LineIndices.push_back(id+1);

 			// triangle indices
            m_HairAsset.m_Triangleindices.push_back(2*id);
            m_HairAsset.m_Triangleindices.push_back(2*id+1);
            m_HairAsset.m_Triangleindices.push_back(2*id+2);
            m_HairAsset.m_Triangleindices.push_back(2*id+2);
            m_HairAsset.m_Triangleindices.push_back(2*id+1);
            m_HairAsset.m_Triangleindices.push_back(2*id+3);
            id++;
		}

		id++;

        for ( int j = 0; j < vertCount; j++ )
        {
			// triangle vertices
            m_HairAsset.m_pTriangleVertices[index] = m_HairStrands[i][j];
            float tVal = m_HairAsset.m_pTriangleVertices[index].texcoord.z;
            m_HairAsset.m_pThicknessCoeffs[index] = sqrt(1.f - tVal * tVal);

            XMFLOAT4 v;

			// temp vertices
            v.x = GetHairs().at(i)->GetVertexArray().at(j).m_Pos.m_X;
            v.y = GetHairs().at(i)->GetVertexArray().at(j).m_Pos.m_Y;
            v.z = GetHairs().at(i)->GetVertexArray().at(j).m_Pos.m_Z;
            v.w = GetHairs().at(i)->GetVertexArray().at(j).m_InvMass;
            m_HairAsset.m_pVertices[index] = v;

			// global rotations
            v.x = GetHairs().at(i)->GetVertexArray().at(j).m_GlobalTransform.GetRotation().m_X;
            v.y = GetHairs().at(i)->GetVertexArray().at(j).m_GlobalTransform.GetRotation().m_Y;
            v.z = GetHairs().at(i)->GetVertexArray().at(j).m_GlobalTransform.GetRotation().m_Z;
            v.w = GetHairs().at(i)->GetVertexArray().at(j).m_GlobalTransform.GetRotation().m_W;    
            m_HairAsset.m_pGlobalRotations[index] = v;

			// local rotations
            v.x = GetHairs().at(i)->GetVertexArray().at(j).m_LocalTransform.GetRotation().m_X;
            v.y = GetHairs().at(i)->GetVertexArray().at(j).m_LocalTransform.GetRotation().m_Y;
            v.z = GetHairs().at(i)->GetVertexArray().at(j).m_LocalTransform.GetRotation().m_Z;
            v.w = GetHairs().at(i)->GetVertexArray().at(j).m_LocalTransform.GetRotation().m_W;
            m_HairAsset.m_pLocalRotations[index] = v;

				// ref vectors
			v.x = GetHairs().at(i)->GetVertexArray().at(j).m_RefVector.m_X;
            v.y = GetHairs().at(i)->GetVertexArray().at(j).m_RefVector.m_Y;
            v.z = GetHairs().at(i)->GetVertexArray().at(j).m_RefVector.m_Z;
            m_HairAsset.m_pRefVectors[index].x = v.x;
            m_HairAsset.m_pRefVectors[index].y = v.y;
            m_HairAsset.m_pRefVectors[index].z = v.z;

			index++;
        }

		int groupId = GetHairs().at(i)->m_GroupID;
        m_HairAsset.m_pHairStrandType[i] = groupId;
		
		if ( GetHairs().at(i)->IsGuideHair() )
		{
			indexGuideHairStrand = i;
			pGuideHair = GetHairs().at(i);
			m_HairAsset.m_pFollowRootOffset[i] = XMFLOAT4(0, 0, 0, (float)indexGuideHairStrand); // forth component is an index to the guide hair strand. For guide hair, it points itself. 
		}
		else
		{
			assert(pGuideHair);
			CVector3D offset = GetHairs().at(i)->GetVertexArray().at(0).m_Pos - pGuideHair->GetVertexArray().at(0).m_Pos;
			m_HairAsset.m_pFollowRootOffset[i] = XMFLOAT4(offset.m_X, offset.m_Y, offset.m_Z, (float)indexGuideHairStrand);	// forth component is an index to the guide hair strand.		
		}
	}

	// transform the hair as needed
	ScaleRotateTranslate();
}

//--------------------------------------------------------------------------------------
//
// WriteBinaryFile
//
// Saves out the hair asset as a binary file for faster loading
//
//--------------------------------------------------------------------------------------
bool CAssetLoader::WriteBinaryFile(const char* filename)
{
	ofstream outFile(filename, ios::binary | ios::trunc);

	if( !outFile.is_open() )
		return false;

	outFile.write((char *)&m_HairAsset.m_NumTotalHairVertices, sizeof(int));
	outFile.write((char *)&m_HairAsset.m_NumTotalHairStrands, sizeof(int));
	outFile.write((char *)&m_HairAsset.m_MaxNumOfVerticesInStrand, sizeof(int));
	outFile.write((char *)&m_HairAsset.m_NumGuideHairVertices, sizeof(int));
	outFile.write((char *)&m_HairAsset.m_NumGuideHairStrands, sizeof(int));
	outFile.write((char *)&m_HairAsset.m_NumFollowHairsPerOneGuideHair, sizeof(int));

	outFile.write((char *)m_HairAsset.m_pHairStrandType, m_HairAsset.m_NumTotalHairStrands * sizeof(int));
	outFile.write((char *)m_HairAsset.m_pRefVectors, m_HairAsset.m_NumTotalHairVertices * sizeof(DirectX::XMFLOAT4));
	outFile.write((char *)m_HairAsset.m_pGlobalRotations, m_HairAsset.m_NumTotalHairVertices * sizeof(DirectX::XMFLOAT4));
	outFile.write((char *)m_HairAsset.m_pLocalRotations, m_HairAsset.m_NumTotalHairVertices * sizeof(DirectX::XMFLOAT4));
	outFile.write((char *)m_HairAsset.m_pVertices, m_HairAsset.m_NumTotalHairVertices * sizeof(DirectX::XMFLOAT4));
	outFile.write((char *)m_HairAsset.m_pTangents, m_HairAsset.m_NumTotalHairVertices * sizeof(DirectX::XMFLOAT4));
	outFile.write((char *)m_HairAsset.m_pTriangleVertices, m_HairAsset.m_NumTotalHairVertices * sizeof(StrandVertex));
	outFile.write((char *)m_HairAsset.m_pThicknessCoeffs, m_HairAsset.m_NumTotalHairVertices * sizeof(float));
	outFile.write((char *)m_HairAsset.m_pFollowRootOffset, m_HairAsset.m_NumTotalHairStrands * sizeof(XMFLOAT4));
	outFile.write((char *)m_HairAsset.m_pRestLengths, m_HairAsset.m_NumTotalHairVertices * sizeof(float));
	outFile.write((char *)&m_HairAsset.m_bSphere, sizeof(BSphere));
	
	int triangleIndicesCount = m_HairAsset.m_Triangleindices.size();
	int lineIndicesCount = m_HairAsset.m_LineIndices.size();

	// Write triangle indices count
	outFile.write((char *)&triangleIndicesCount, sizeof(int));
	
	// Convert triangle indices vector to integer array
	int* triangleIndices = new int[triangleIndicesCount];

	for (int i = 0; i < triangleIndicesCount; i++)
	{
		triangleIndices[i] = m_HairAsset.m_Triangleindices[i];
	}
	
	// Write triangle indices
	outFile.write((char *)triangleIndices, triangleIndicesCount * sizeof(int));

	// Write line indices count
	outFile.write((char *)&lineIndicesCount, sizeof(int));

	// Convert line indices vector to integer array
	int* lineIndices = new int[lineIndicesCount];

	for (int i = 0; i < lineIndicesCount; i++)
	{
		lineIndices[i] = m_HairAsset.m_LineIndices[i];
	}

	// Write line indices
	outFile.write((char *)lineIndices, lineIndicesCount * sizeof(int));
	/*outFile << m_HairAsset.m_Triangleindices.size();
	outFile.write((char *)&m_HairAsset.m_Triangleindices[0], m_HairAsset.m_Triangleindices.size() * sizeof(int));
	outFile << m_HairAsset.m_LineIndices.size();
	outFile.write((char *)&m_HairAsset.m_LineIndices[0], m_HairAsset.m_LineIndices.size() * sizeof(int));*/

	outFile.close();

	return true;
}

//--------------------------------------------------------------------------------------
//
// ProceduralHairParams
//
// For generating hair procedurally
//
//--------------------------------------------------------------------------------------
bool CAssetLoader::ProceduralHairParams::LoadFromFile(const char* filename)
{
	ifstream inFile(filename);
	string sLine;
	vector<string> sTokens;

	if ( !inFile.is_open() )
		return false;

	while(inFile.good())
	{
		getline(inFile, sLine);
		sTokens.clear(); 
		int numFound = StringTokenizer(sLine, string(" "), sTokens, false);

		if ( numFound == 2 )
		{
			vector <string>::iterator iter;
			string sToken; 

			iter = sTokens.begin();
			sToken = *(iter++);

			if(sToken == "numGrassesPerTri")
			{
				sToken = *(iter);
				numGrassesPerTri = atoi(sToken.c_str());
			}
			else if(sToken == "restLength")
			{
				sToken = *(iter);
				restLength = (float) atof(sToken.c_str());
			}
		}
	}
	return true;
}

//--------------------------------------------------------------------------------------
//
// WriteStrands
//
// Writes out the strands to a .tfx file
//
//--------------------------------------------------------------------------------------
bool CAssetLoader::WriteStrands(const char* filename)
{
	ofstream outFile(filename);

	if( !outFile.is_open() )
		return false;

	size_t numStrands = m_Hairs.size();

	outFile << "numStrands " << numStrands << "\n";
	outFile << "is sorted 1 \n";

	for(size_t iStrand = 0; iStrand < numStrands; ++iStrand)
	{
		const CHairAsset& group = * (m_Hairs[iStrand]);

		const std::vector<HairVertex>& vArray = group.GetVertexArray();
		size_t numVerts = vArray.size();

		outFile << "strand " << iStrand << " numVerts " << numVerts << " texcoord 0.000 0.000\n";

		for(size_t iVert = 0; iVert < numVerts; ++iVert)
		{
			const CVector3D& pos = vArray[iVert].m_Pos;
			outFile << pos.m_X << " " << pos.m_Y << " " << pos.m_Z << "\n";
		}
	}

	outFile.close();
	return true;
	

}


//--------------------------------------------------------------------------------------
//
// StringTokenizer
//
// Parses the strings into tokens
//
//--------------------------------------------------------------------------------------
int StringTokenizer(const string& input, const string& delimiter, 
				vector<string>& results, bool includeEmpties)
{
    int iPos = 0;
    int newPos = -1;
    int sizeS2 = (int)delimiter.size();
    int isize = (int)input.size();

    if( isize == 0 || sizeS2 == 0 )
        return 0;

    vector<int> positions;
    newPos = (int)input.find(delimiter, 0);

    if( newPos < 0 )
        return 0; 

    int numFound = 0;

    while( newPos >= iPos )
    {
        numFound++;
        positions.push_back(newPos);
        iPos = newPos;
        newPos = (int)input.find (delimiter, iPos+sizeS2);
    }

    if( numFound == 0 )
        return 0;

    for( int i=0; i <= (int)positions.size(); ++i )
    {
        string s("");

        if( i == 0 ) 
        { 
            s = input.substr( i, positions[i] ); 
        }
		else
		{
			int offset = positions[i-1] + sizeS2;

			if( offset < isize )
			{
				if( i == (int)positions.size() )
				{
					s = input.substr(offset);
				}
				else if( i > 0 )
				{
					s = input.substr( positions[i-1] + sizeS2, 
						  positions[i] - positions[i-1] - sizeS2 );
				}
			}
		}

        if( includeEmpties || ( s.size() > 0 ) )
		{
            results.push_back(s);
		}
    }

    return ++numFound;
}