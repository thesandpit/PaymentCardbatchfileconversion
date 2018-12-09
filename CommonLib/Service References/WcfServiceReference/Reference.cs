﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CommonLib.WcfServiceReference {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://Microsoft.ServiceModel.Samples", ConfigurationName="WcfServiceReference.IWcfService", SessionMode=System.ServiceModel.SessionMode.NotAllowed)]
    public interface IWcfService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://Microsoft.ServiceModel.Samples/IWcfService/RunApp", ReplyAction="http://Microsoft.ServiceModel.Samples/IWcfService/RunAppResponse")]
        bool RunApp(string appName, string para);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://Microsoft.ServiceModel.Samples/IWcfService/RunApp", ReplyAction="http://Microsoft.ServiceModel.Samples/IWcfService/RunAppResponse")]
        System.Threading.Tasks.Task<bool> RunAppAsync(string appName, string para);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://Microsoft.ServiceModel.Samples/IWcfService/RunInteractiveProcess", ReplyAction="http://Microsoft.ServiceModel.Samples/IWcfService/RunInteractiveProcessResponse")]
        bool RunInteractiveProcess(string exePath, string para);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://Microsoft.ServiceModel.Samples/IWcfService/RunInteractiveProcess", ReplyAction="http://Microsoft.ServiceModel.Samples/IWcfService/RunInteractiveProcessResponse")]
        System.Threading.Tasks.Task<bool> RunInteractiveProcessAsync(string exePath, string para);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IWcfServiceChannel : CommonLib.WcfServiceReference.IWcfService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class WcfServiceClient : System.ServiceModel.ClientBase<CommonLib.WcfServiceReference.IWcfService>, CommonLib.WcfServiceReference.IWcfService {
        
        public WcfServiceClient() {
        }
        
        public WcfServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public WcfServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public WcfServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public WcfServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public bool RunApp(string appName, string para) {
            return base.Channel.RunApp(appName, para);
        }
        
        public System.Threading.Tasks.Task<bool> RunAppAsync(string appName, string para) {
            return base.Channel.RunAppAsync(appName, para);
        }
        
        public bool RunInteractiveProcess(string exePath, string para) {
            return base.Channel.RunInteractiveProcess(exePath, para);
        }
        
        public System.Threading.Tasks.Task<bool> RunInteractiveProcessAsync(string exePath, string para) {
            return base.Channel.RunInteractiveProcessAsync(exePath, para);
        }
    }
}
