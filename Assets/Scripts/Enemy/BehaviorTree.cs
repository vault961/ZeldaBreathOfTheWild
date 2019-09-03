using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 델리게이트
public delegate bool delRun();

// 트리 모든 노드의 기본이 되는 추상 클래스
public abstract class Node
{
    public abstract bool Run();
}

// 다른 노드를 List로 가질수 있는 노드 (Sequence, Selector가 이에 해당합니다)
public class CompositeNode : Node
{
    private List<Node> child = new List<Node>();
    public List<Node> GetChild() { return child; }

    public CompositeNode AddChild(Node node) { child.Add(node); return this;}

    public override bool Run()
    {
        throw new System.NotImplementedException();
    }
}

// 행동 노드 (트리 생성에 사용하는 클래스입니다)
public class ActionNode : Node
{
    public delRun runMethod;

    // 트리 생성에 사용하는 함수
    public static ActionNode Make(delRun algo)
    {
        ActionNode temp = new ActionNode();
        temp.runMethod = algo;
        return temp;
    }

    public override bool Run() { return runMethod(); }
}

// 시퀸스 : 자식 노드 중 하나라도 FALSE가 있다면 FALSE 반환
public class Sequence : CompositeNode
{
    public static Sequence Make() { return new Sequence(); }

    public override bool Run()
    {
        foreach (Node node in GetChild()) { if (!node.Run()) return false; }
        return true;
    }
}

// 셀렉터 : 자식 노드 중 하나라도 TRUE가 있다면 TRUE 반환
public class Selector : CompositeNode
{
    public static Selector Make() { return new Selector(); }

    public override bool Run()
    {
        foreach (Node node in GetChild()) { if (node.Run()) return true; }
        return false;
    }
}