using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace com.tinylabproductions.TLPLib.Editor.VisualTimelineTemplate {
  public class TweenerTemp : MonoBehaviour {
    public List<Sequence> sequences;


    private void Start(){
      for (int i=0; i< sequences.Count; i++) {
        Sequence sequence=sequences[i];
        if(sequence.playAutomatically){
          sequence.Play();
        }
      }
    }

    private void Update(){
      for (int i=0; i< sequences.Count; i++) {
        sequences[i].Update(gameObject);
      }
    }

    public void Play(string name){
      for (int i=0; i< sequences.Count; i++) {
        Sequence sequence=sequences[i];
        if(sequence.name == name){
          sequence.Play();
        }
      }
    }

    public void Stop(){
      for (int i=0; i< sequences.Count; i++) {
        Sequence sequence=sequences[i];
        sequence.Stop();
      }
    }

    public void Stop(string name){
      for (int i=0; i< sequences.Count; i++) {
        Sequence sequence=sequences[i];
        if(sequence.name == name){
          sequence.Stop();
        }
      }
    }

    public bool IsPlaying(string name){
      for (int i=0; i< sequences.Count; i++) {
        Sequence sequence=sequences[i];
        if(sequence.name == name){
          return !sequence.stop;
        }
      }
      return false;
    }
  }


}
