using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.sequences;

namespace com.tinylabproductions.TLPLib.Editor.VisualTimelineTemplate {
  public class TweenerTemp : MonoBehaviour {
    public List<SequenceTemp> sequences;
    public List<SerializedTweenTimeline> timelines;


    private void Start(){
      for (int i=0; i< sequences.Count; i++) {
        SequenceTemp sequenceTemp=sequences[i];
        if(sequenceTemp.playAutomatically){
          sequenceTemp.Play();
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
        SequenceTemp sequenceTemp=sequences[i];
        if(sequenceTemp.name == name){
          sequenceTemp.Play();
        }
      }
    }

    public void Stop(){
      for (int i=0; i< sequences.Count; i++) {
        SequenceTemp sequenceTemp=sequences[i];
        sequenceTemp.Stop();
      }
    }

    public void Stop(string name){
      for (int i=0; i< sequences.Count; i++) {
        SequenceTemp sequenceTemp=sequences[i];
        if(sequenceTemp.name == name){
          sequenceTemp.Stop();
        }
      }
    }

    public bool IsPlaying(string name){
      for (int i=0; i< sequences.Count; i++) {
        SequenceTemp sequenceTemp=sequences[i];
        if(sequenceTemp.name == name){
          return !sequenceTemp.stop;
        }
      }
      return false;
    }
  }


}
