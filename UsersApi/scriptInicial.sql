-- Script inicial para Users API
-- Insertar roles básicos

-- Rol Admin
INSERT INTO roles (codigo, nombre, descripcion, esta_activo, usuario_creacion, fecha_creacion)
VALUES ('ADMIN', 'admin', 'Administrador', true, 'sistema', NOW())
ON CONFLICT (codigo) DO NOTHING;

-- Rol TOM
INSERT INTO roles (codigo, nombre, descripcion, esta_activo, usuario_creacion, fecha_creacion)
VALUES ('TOM', 'TOM', 'Técnico', true, 'sistema', NOW())
ON CONFLICT (codigo) DO NOTHING;

-- Rol Verificador
INSERT INTO roles (codigo, nombre, descripcion, esta_activo, usuario_creacion, fecha_creacion)
VALUES ('VERIF', 'verificador', 'Verificador', true, 'sistema', NOW())
ON CONFLICT (codigo) DO NOTHING;

-- Rol Cliente
INSERT INTO roles (codigo, nombre, descripcion, esta_activo, usuario_creacion, fecha_creacion)
VALUES ('CLIENT', 'cliente', 'Cliente', true, 'sistema', NOW())
ON CONFLICT (codigo) DO NOTHING;

-- Usuario administrador
INSERT INTO "Usuarios" (
    nombre,
    codigo_usuario,
    contrasenia,
    intentos_fallidos,
    esta_activo,
    roles,
    usuario_creacion,
    fecha_creacion
)
VALUES (
    'Administrador',
    'admin',
    '$argon2id$v=19$m=65536,t=4,p=4$TwQ6oxWg/w8JTLh6E+iYow$su0tN1gvh9ykV1huL9kXdAes5wt6AX1WYgcaBSZ0PWQ',
    0,
    true,
    'admin',
    'sistema',
    NOW()
)
ON CONFLICT (codigo_usuario) DO NOTHING;

-- Asignar rol ADMIN al usuario administrador
INSERT INTO usuario_roles (id_usuario, id_rol, usuario_creacion, fecha_creacion)
SELECT u.id_usuario, r.id_rol, 'sistema', NOW()
FROM "Usuarios" u, roles r
WHERE u.codigo_usuario = 'admin' AND r.codigo = 'ADMIN'
ON CONFLICT (id_usuario, id_rol) DO NOTHING;