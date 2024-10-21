using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Recipes
{
    // 조합법 리스트 (조합법과 결과 아이템을 매핑)
    private readonly List<(Dictionary<string, int>, string)> craftingRecipes = new List<(Dictionary<string, int>, string)>();

    public Recipes()
    {
        // 받침대
        craftingRecipes.Add((new Dictionary<string, int>
        {
            { "WoodenPlank", 1 },
            { "Clay", 1 }
        }, "Pedestal_basic"));

        // 몸통
        craftingRecipes.Add((new Dictionary<string, int>
        {
            { "WoodenPlank", 3 },
            { "Clay", 1 }
        }, "Body_basic"));

        // 기본 발사기
        craftingRecipes.Add((new Dictionary<string, int>
        {
            { "WoodenPlank", 2 },
            { "Sand", 1 }
        }, "Gun_basic"));

        // 연사 발사기
        craftingRecipes.Add((new Dictionary<string, int>
        {
            { "Gun_basic", 2 }
        }, "Gun_double"));

        // 강화 받침대
        craftingRecipes.Add((new Dictionary<string, int>
        {
            { "Pedestal_basic", 1 },
            { "ForgedSteel", 1 },
            { "Clay", 1 }
        }, "Pedestal_enforce"));
        
        // 강화 몸통
        craftingRecipes.Add((new Dictionary<string, int>
        {
            { "Body_basic", 1 },
            { "ForgedSteel", 1 },
            { "Clay", 1 }
        }, "Body_enforce"));

        // 화염 방사기
        craftingRecipes.Add((new Dictionary<string, int>
        {
            { "Gun_double", 1 },
            { "ForgedSteel", 1 },
            { "Sand", 1 }
        }, "Gun_fire"));

        // 얼음 발사기
        craftingRecipes.Add((new Dictionary<string, int>
        {
            { "Gun_double", 1 },
            { "ForgedSteel", 1 },
            { "MindStone", 1 }
        }, "Gun_ice"));

        // 배리어
        craftingRecipes.Add((new Dictionary<string, int>
        {
            { "ForgedSteel", 4 },
            { "Clay", 2 },
            { "MindStone", 1 }
        }, "Barrier"));

        //// 테스트용
        //craftingRecipes.Add((new Dictionary<string, int>
        //{
        //    { "Wood", 3 }
        //}, "Gun Variant"));

        //// 테스트용
        //craftingRecipes.Add((new Dictionary<string, int>
        //{
        //    { "Wood", 2 }
        //}, "Body"));

        //// 테스트용
        //craftingRecipes.Add((new Dictionary<string, int>
        //{
        //    { "Wood", 1 }
        //}, "Pedestal"));

        // 탁호준 테스트용
        //craftingRecipes.Add((new Dictionary<string, int>
        //{
        //    { "Wood", 3 }
        //}, "Gun Variant5"));
        ////// 테스트용
        //craftingRecipes.Add((new Dictionary<string, int>
        //{
        //    { "Wood", 2 }
        //}, "Body"));

        ////// 테스트용
        //craftingRecipes.Add((new Dictionary<string, int>
        //{
        //    { "Wood", 1 }
        //}, "Pedestal"));

        //// 테스트용
        //craftingRecipes.Add((new Dictionary<string, int>
        //{
        //    { "Sand", 1 }
        //}, "Pedestal_enforce"));

        ////// 테스트용
        //craftingRecipes.Add((new Dictionary<string, int>
        //{
        //    { "Sand", 2 }
        //}, "Body_enforce"));

        ////// 테스트용
        //craftingRecipes.Add((new Dictionary<string, int>
        //{
        //    { "Sand", 3 }
        //}, "Enforce_head"));

        ////테스트용
        //craftingRecipes.Add((new Dictionary<string, int>
        //{
        //    { "Wood", 1 }
        //}, "Gun_ice"));

    }

    // 테이블 위의 재료들로 조합 가능한지 확인하고, 완성품을 반환하는 함수
    public string CheckCrafting(Stack<GameObject> tableStack)
    {
        if (tableStack.Count == 0) return "";

        Stack<GameObject> materialsOnTable = new Stack<GameObject>(tableStack.Reverse());
        // 테이블에 있는 재료들의 이름과 개수를 Dictionary로 변환
        Dictionary<string, int> materialsCounts = new Dictionary<string, int>();

        foreach (GameObject material in materialsOnTable)
        {
            string name = material.name.Replace("(Clone)", "").Trim(); // 프리팹의 이름 사용
            if (materialsCounts.ContainsKey(name))
            {
                materialsCounts[name]++;
            }
            else
            {
                materialsCounts[name] = 1;
            }
        }

        // 재료가 조합법에 맞는지 확인
        foreach (var recipe in craftingRecipes)
        {
            if (RecipeMatches(materialsCounts, recipe.Item1))
            {
                // 조합법이 일치하면 해당 완성품을 반환
                return recipe.Item2;
            }
        }

        // 어떤 조합법에도 해당하지 않으면 빈 문자열 반환
        return "";
    }

    // 주어진 재료와 조합법이 일치하는지 확인하는 함수
    private bool RecipeMatches(Dictionary<string, int> materials, Dictionary<string, int> recipe)
    {
        // 재료 종류와 개수가 정확히 일치하는지 확인
        foreach (var ingredient in recipe)
        {
            string ingredientName = ingredient.Key;
            int requiredCount = ingredient.Value;

            // 재료의 수가 일치하지 않는 경우
            if (!materials.ContainsKey(ingredientName) || materials[ingredientName] != requiredCount)
            {
                return false;
            }
        }

        // 테이블 위의 재료가 조합법에 맞는 경우
        // 조합법의 모든 재료가 정확히 일치하는지 확인
        foreach (var material in materials)
        {
            if (!recipe.ContainsKey(material.Key))
            {
                // 조합법에 없는 재료가 테이블 위에 있는 경우
                return false;
            }
        }

        // 조합법의 모든 재료가 정확히 일치하면 true 반환
        return true;
    }
}
