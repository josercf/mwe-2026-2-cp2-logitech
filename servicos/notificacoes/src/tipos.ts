/**
 * Contratos do serviço de notificações da LogiTech.
 *
 * `Enviador` é o contrato comum de canais e decoradores: quem chama não
 * distingue um enviador de canal de um enviador embrulhado.
 */

/** Uma notificação a ser entregue a um destinatário da LogiTech. */
export interface Notificacao {
  /** Canal de entrega registrado: `email`, `sms` ou `whatsapp`. */
  canal: string;
  /** Endereço, telefone ou identificador do destinatário. */
  destinatario: string;
  /** Corpo da mensagem, já pronto para envio. */
  mensagem: string;
}

/** O que o serviço devolve depois de tentar entregar uma notificação. */
export interface ResultadoEnvio {
  entregue: boolean;
  canal: string;
  /** Quantas tentativas foram feitas. O enviador de canal sempre devolve 1. */
  tentativas: number;
  /** Protocolo devolvido pelo provedor do canal. */
  identificador: string;
}

export interface Enviador {
  readonly canal: string;
  enviar(notificacao: Notificacao): Promise<ResultadoEnvio>;
}

/** Destino das linhas de log. */
export interface Registrador {
  registrar(linha: string): void;
}
