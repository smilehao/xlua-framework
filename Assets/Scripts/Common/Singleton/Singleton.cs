using System;
using UnityEngine;

public abstract class Singleton<T> where T : class, new()
{
	private static T m_instance;
	public static T instance
	{
		get
        {
            if (Singleton<T>.m_instance == null)
		    {
			    Singleton<T>.m_instance = Activator.CreateInstance<T>();
			    if (Singleton<T>.m_instance != null)
			    {
                    (Singleton<T>.m_instance as Singleton<T>).Init();
			    }
		    }

            return Singleton<T>.m_instance;
        }
	}

	public static void Release()
	{
		if (Singleton<T>.m_instance != null)
		{
			Singleton<T>.m_instance = (T)((object)null);
		}
	}

    public virtual void Init()
    {

    }

    public abstract void Dispose();

}
